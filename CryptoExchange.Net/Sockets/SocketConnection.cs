using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.Objects;
using System.Net.WebSockets;
using System.IO;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Converters;
using System.Text;
using System.Runtime;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// A single socket connection to the server
    /// </summary>
    public class SocketConnection
    {
        /// <summary>
        /// Connection lost event
        /// </summary>
        public event Action? ConnectionLost;

        /// <summary>
        /// Connection closed and no reconnect is happening
        /// </summary>
        public event Action? ConnectionClosed;

        /// <summary>
        /// Connecting restored event
        /// </summary>
        public event Action<TimeSpan>? ConnectionRestored;

        /// <summary>
        /// The connection is paused event
        /// </summary>
        public event Action? ActivityPaused;

        /// <summary>
        /// The connection is unpaused event
        /// </summary>
        public event Action? ActivityUnpaused;

        /// <summary>
        /// Unhandled message event
        /// </summary>
        public event Action<ParsedMessage>? UnhandledMessage;

        /// <summary>
        /// Unparsed message event
        /// </summary>
        public event Action<byte[]>? UnparsedMessage;

        /// <summary>
        /// The amount of listeners on this connection
        /// </summary>
        public int UserListenerCount
        {
            get { lock (_listenerLock)
                return _messageIdentifierListeners.Values.Count(h => h.UserSubscription); }
        }

        /// <summary>
        /// Get a copy of the current message listeners
        /// </summary>
        public Subscription[] Subscriptions
        {
            get
            {
                lock (_listenerLock)
                    return _messageIdentifierListeners.Values.Where(h => h.UserSubscription).ToArray();
            }
        }

        /// <summary>
        /// If the connection has been authenticated
        /// </summary>
        public bool Authenticated { get; internal set; }

        /// <summary>
        /// If connection is made
        /// </summary>
        public bool Connected => _socket.IsOpen;

        /// <summary>
        /// The unique ID of the socket
        /// </summary>
        public int SocketId => _socket.Id;

        /// <summary>
        /// The current kilobytes per second of data being received, averaged over the last 3 seconds
        /// </summary>
        public double IncomingKbps => _socket.IncomingKbps;

        /// <summary>
        /// The connection uri
        /// </summary>
        public Uri ConnectionUri => _socket.Uri;

        /// <summary>
        /// The API client the connection is for
        /// </summary>
        public SocketApiClient ApiClient { get; set; }

        /// <summary>
        /// Time of disconnecting
        /// </summary>
        public DateTime? DisconnectTime { get; set; }

        /// <summary>
        /// Tag for identificaion
        /// </summary>
        public string Tag { get; set; }
        
        /// <summary>
        /// Additional properties for this connection
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// If activity is paused
        /// </summary>
        public bool PausedActivity
        {
            get => _pausedActivity;
            set
            {
                if (_pausedActivity != value)
                {
                    _pausedActivity = value;
                    _logger.Log(LogLevel.Information, $"Socket {SocketId} Paused activity: " + value);
                    if(_pausedActivity) _ = Task.Run(() => ActivityPaused?.Invoke());
                    else _ = Task.Run(() => ActivityUnpaused?.Invoke());
                }
            }
        }

        /// <summary>
        /// Status of the socket connection
        /// </summary>
        public SocketStatus Status
        {
            get => _status;
            private set
            {
                if (_status == value)
                    return;

                var oldStatus = _status;
                _status = value;
                _logger.Log(LogLevel.Debug, $"Socket {SocketId} status changed from {oldStatus} to {_status}");
            }
        }

        private bool _pausedActivity;
        private readonly List<BasePendingRequest> _pendingRequests;
        private readonly List<Subscription> _messageListeners;
        private readonly Dictionary<string, Subscription> _messageIdentifierListeners;

        private readonly object _listenerLock = new();
        private readonly ILogger _logger;

        private SocketStatus _status;

        /// <summary>
        /// The underlying websocket
        /// </summary>
        private readonly IWebsocket _socket;

        /// <summary>
        /// New socket connection
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="apiClient">The api client</param>
        /// <param name="socket">The socket</param>
        /// <param name="tag"></param>
        public SocketConnection(ILogger logger, SocketApiClient apiClient, IWebsocket socket, string tag)
        {
            _logger = logger;
            ApiClient = apiClient;
            Tag = tag;
            Properties = new Dictionary<string, object>();

            _pendingRequests = new List<BasePendingRequest>();
            _messageListeners = new List<Subscription>();
            _messageIdentifierListeners = new Dictionary<string, Subscription>();

            _socket = socket;
            _socket.OnStreamMessage += HandleStreamMessage;
            _socket.OnRequestSent += HandleRequestSent;
            _socket.OnOpen += HandleOpen;
            _socket.OnClose += HandleClose;
            _socket.OnReconnecting += HandleReconnecting;
            _socket.OnReconnected += HandleReconnected;
            _socket.OnError += HandleError;
            _socket.GetReconnectionUrl = GetReconnectionUrlAsync;
        }

        /// <summary>
        /// Handler for a socket opening
        /// </summary>
        protected virtual void HandleOpen()
        {
            Status = SocketStatus.Connected;
            PausedActivity = false;
        }

        /// <summary>
        /// Handler for a socket closing without reconnect
        /// </summary>
        protected virtual void HandleClose()
        {
            Status = SocketStatus.Closed;
            Authenticated = false;
            lock(_listenerLock)
            {
                foreach (var listener in _messageListeners)
                    listener.Confirmed = false;
            }    
            Task.Run(() => ConnectionClosed?.Invoke());
        }

        /// <summary>
        /// Handler for a socket losing conenction and starting reconnect
        /// </summary>
        protected virtual void HandleReconnecting()
        {
            Status = SocketStatus.Reconnecting;
            DisconnectTime = DateTime.UtcNow;
            Authenticated = false;
            lock (_listenerLock)
            {
                foreach (var listener in _messageListeners)
                    listener.Confirmed = false;
            }

            _ = Task.Run(() => ConnectionLost?.Invoke());
        }

        /// <summary>
        /// Get the url to connect to when reconnecting
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<Uri?> GetReconnectionUrlAsync()
        {
            return await ApiClient.GetReconnectUriAsync(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Handler for a socket which has reconnected
        /// </summary>
        protected virtual async void HandleReconnected()
        {
            Status = SocketStatus.Resubscribing;
            lock (_messageListeners)
            {
                foreach (var pendingRequest in _pendingRequests.ToList())
                {
                    pendingRequest.Fail("Connection interupted");
                    // Remove?
                }
            }

            var reconnectSuccessful = await ProcessReconnectAsync().ConfigureAwait(false);
            if (!reconnectSuccessful)
            {
                _logger.Log(LogLevel.Warning, $"Socket {SocketId} Failed reconnect processing: {reconnectSuccessful.Error}, reconnecting again");
                await _socket.ReconnectAsync().ConfigureAwait(false);
            }
            else
            {
                Status = SocketStatus.Connected;
                _ = Task.Run(() =>
                {
                    ConnectionRestored?.Invoke(DateTime.UtcNow - DisconnectTime!.Value);
                    DisconnectTime = null;
                });
            }
        }

        /// <summary>
        /// Handler for an error on a websocket
        /// </summary>
        /// <param name="e">The exception</param>
        protected virtual void HandleError(Exception e)
        {
            if (e is WebSocketException wse)
                _logger.Log(LogLevel.Warning, $"Socket {SocketId} error: Websocket error code {wse.WebSocketErrorCode}, details: " + e.ToLogString());
            else
                _logger.Log(LogLevel.Warning, $"Socket {SocketId} error: " + e.ToLogString());
        }

        /// <summary>
        /// Handler for whenever a request is sent over the websocket
        /// </summary>
        /// <param name="requestId">Id of the request sent</param>
        protected virtual void HandleRequestSent(int requestId)
        {
            BasePendingRequest pendingRequest;
            lock (_pendingRequests)
                pendingRequest = _pendingRequests.SingleOrDefault(p => p.Id == requestId);

            if (pendingRequest == null)
            {
                _logger.Log(LogLevel.Debug, $"Socket {SocketId} - msg {requestId} - message sent, but not pending");
                return;
            }

            pendingRequest.IsSend();
        }

        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual async Task HandleStreamMessage(Stream stream)
        {
            var timestamp = DateTime.UtcNow;
            TimeSpan userCodeDuration = TimeSpan.Zero;

            List<Subscription> listeners;
            lock (_listenerLock)
                listeners = _messageListeners.OrderByDescending(x => !x.UserSubscription).ToList();

            var result = ApiClient.StreamConverter.ReadJson(stream, _pendingRequests, listeners, ApiClient.ApiOptions.OutputOriginalData ?? ApiClient.ClientOptions.OutputOriginalData);
            if(result == null)
            {
                stream.Position = 0;
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                UnparsedMessage?.Invoke(buffer);
                return;
            }

            if (result.OriginalData != null)
                _logger.LogDebug($"Socket {SocketId} Data received: {result.OriginalData}");

            if (result.Data == null)
            {
                _logger.LogWarning("Message not matched to type");
                return;
            }

            // TODO lock
            if (_messageIdentifierListeners.TryGetValue(result.Identifier.ToLowerInvariant(), out var idListener))
            {
                // Matched based on identifier
                var userSw = Stopwatch.StartNew();
                var dataEvent = new DataEvent<ParsedMessage>(result, null, result.OriginalData, DateTime.UtcNow, null);
                await idListener.HandleEventAsync(dataEvent).ConfigureAwait(false);
                userSw.Stop();
                return;
            }

            List<BasePendingRequest> pendingRequests;
            lock (_pendingRequests)
                pendingRequests = _pendingRequests.ToList();

            foreach (var pendingRequest in pendingRequests)
            {
                if (pendingRequest.MessageMatchesHandler(result))
                {
                    lock (_pendingRequests)
                        _pendingRequests.Remove(pendingRequest);

                    if (pendingRequest.Completed)
                    {
                        // Answer to a timed out request, unsub if it is a subscription request
                        // TODO
                        //if (pendingRequest.MessageListener != null)
                        //{
                        //    _logger.Log(LogLevel.Warning, $"Socket {SocketId} Received subscription info after request timed out; unsubscribing. Consider increasing the RequestTimeout");
                        //    _ = UnsubscribeAsync(pendingRequest.MessageListener).ConfigureAwait(false);
                        //}
                    }
                    else
                    {
                        _logger.Log(LogLevel.Trace, $"Socket {SocketId} - msg {pendingRequest.Id} - received data matched to pending request");
                        await pendingRequest.ProcessAsync(result).ConfigureAwait(false);
                    }

                    return;
                }
            }

            stream.Position = 0;
            var unhandledBuffer = new byte[stream.Length];
            stream.Read(unhandledBuffer, 0, unhandledBuffer.Length);
            
            _logger.Log(LogLevel.Warning, $"Socket {SocketId} Message not handled: " + Encoding.UTF8.GetString(unhandledBuffer));
            UnhandledMessage?.Invoke(result);
        }    

        /// <summary>
        /// Connect the websocket
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync() => await _socket.ConnectAsync().ConfigureAwait(false);

        /// <summary>
        /// Retrieve the underlying socket
        /// </summary>
        /// <returns></returns>
        public IWebsocket GetSocket() => _socket;

        /// <summary>
        /// Trigger a reconnect of the socket connection
        /// </summary>
        /// <returns></returns>
        public async Task TriggerReconnectAsync() => await _socket.ReconnectAsync().ConfigureAwait(false);

        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            if (Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            if (ApiClient.socketConnections.ContainsKey(SocketId))
                ApiClient.socketConnections.TryRemove(SocketId, out _);

            lock (_listenerLock)
            {
                foreach (var listener in _messageListeners)
                {
                    if (listener.CancellationTokenRegistration.HasValue)
                        listener.CancellationTokenRegistration.Value.Dispose();
                }
            }

            await _socket.CloseAsync().ConfigureAwait(false);
            _socket.Dispose();
        }

        /// <summary>
        /// Close a listener on this connection. If all listener on this connection are closed the connection gets closed as well
        /// </summary>
        /// <param name="listener">Listener to close</param>
        /// <returns></returns>
        public async Task CloseAsync(Subscription subscription)
        {
            lock (_listenerLock)
            {
                if (!_messageListeners.Contains(subscription))
                    return;

                subscription.Closed = true;
            }

            if (Status == SocketStatus.Closing || Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            _logger.Log(LogLevel.Debug, $"Socket {SocketId} closing listener {subscription.Id}");
            if (subscription.CancellationTokenRegistration.HasValue)
                subscription.CancellationTokenRegistration.Value.Dispose();

            if (subscription.Confirmed && _socket.IsOpen)
                await UnsubscribeAsync(subscription).ConfigureAwait(false);

            bool shouldCloseConnection;
            lock (_listenerLock)
            {
                if (Status == SocketStatus.Closing)
                {
                    _logger.Log(LogLevel.Debug, $"Socket {SocketId} already closing");
                    return;
                }

                shouldCloseConnection = _messageIdentifierListeners.All(r => !r.Value.UserSubscription || r.Value.Closed);
                if (shouldCloseConnection)
                    Status = SocketStatus.Closing;
            }

            if (shouldCloseConnection)
            {
                _logger.Log(LogLevel.Debug, $"Socket {SocketId} closing as there are no more listeners");
                await CloseAsync().ConfigureAwait(false);
            }

            lock (_listenerLock)
            {
                _messageListeners.Remove(subscription);
                foreach (var id in subscription.Identifiers)
                    _messageIdentifierListeners.Remove(id);
            }
        }

        /// <summary>
        /// Dispose the connection
        /// </summary>
        public void Dispose()
        {
            Status = SocketStatus.Disposed;
            _socket.Dispose();
        }

        /// <summary>
        /// Add a listener to this connection
        /// </summary>
        /// <param name="listener"></param>
        public bool AddListener(Subscription subscription)
        {
            lock (_listenerLock)
            {
                if (Status != SocketStatus.None && Status != SocketStatus.Connected)
                    return false;

                _messageListeners.Add(subscription);
                if (subscription.Identifiers != null)
                {
                    foreach (var id in subscription.Identifiers)
                        _messageIdentifierListeners.Add(id.ToLowerInvariant(), subscription);
                }

                if (subscription.UserSubscription)
                    _logger.Log(LogLevel.Debug, $"Socket {SocketId} adding new listener with id {subscription.Id}, total listeners on connection: {_messageListeners.Count(s => s.UserSubscription)}");
                return true;
            }
        }

        /// <summary>
        /// Get a listener on this connection by id
        /// </summary>
        /// <param name="id"></param>
        public Subscription? GetListener(int id)
        {
            lock (_listenerLock)
                return _messageListeners.SingleOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Get a listener on this connection by its subscribe request
        /// </summary>
        /// <param name="predicate">Filter for a request</param>
        /// <returns></returns>
        public Subscription? GetListenerByRequest(Func<object?, bool> predicate)
        {
            lock(_listenerLock)
                return _messageListeners.SingleOrDefault(s => predicate(s));
        }

        public virtual async Task<CallResult<T>> SendAndWaitQueryAsync<T>(Query<T> query)
        {
            var pendingRequest = PendingRequest<T>.CreateForQuery(query);
            await SendAndWaitAsync(pendingRequest, query.Weight).ConfigureAwait(false);
            return pendingRequest.Result;
        }

        public virtual async Task<CallResult> SendAndWaitQueryAsync(Query query)
        {
            var pendingRequest = PendingRequest.CreateForQuery(query);
            await SendAndWaitAsync(pendingRequest, query.Weight).ConfigureAwait(false);
            return pendingRequest.Result;
        }

        public virtual async Task<CallResult> SendAndWaitSubAsync(Subscription subscription)
        {
            var pendingRequest = PendingRequest.CreateForSubRequest(subscription);
            await SendAndWaitAsync(pendingRequest, 1).ConfigureAwait(false);
            return pendingRequest.Result;
        }

        public virtual async Task<CallResult> SendAndWaitUnsubAsync(Subscription subscription)
        {
            var pendingRequest = PendingRequest.CreateForUnsubRequest(subscription);
            await SendAndWaitAsync(pendingRequest, 1).ConfigureAwait(false);
            return pendingRequest.Result;
        }

        private async Task SendAndWaitAsync(BasePendingRequest pending, int weight)
        {
            lock (_messageListeners)
            {
                _pendingRequests.Add(pending);
            }

            var sendOk = Send(pending.Id, pending.Request, weight);
            if (!sendOk)
            {
                pending.Fail("Failed to send");
                return;
            }

            while (true)
            {
                if (!_socket.IsOpen)
                {
                    pending.Fail("Socket not open");
                    return;
                }

                if (pending.Completed)
                    return;

                await pending.Event.WaitAsync(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);

                if (pending.Completed)
                    return;
            }
        }

        ///// <summary>
        ///// Send data and wait for an answer
        ///// </summary>
        ///// <typeparam name="T">The data type expected in response</typeparam>
        ///// <param name="obj">The object to send</param>
        ///// <param name="timeout">The timeout for response</param>
        ///// <param name="listener">Listener if this is a subscribe request</param>
        ///// <param name="handler">The response handler</param>
        ///// <param name="weight">The weight of the message</param>
        ///// <returns></returns>
        //public virtual async Task SendAndWaitAsync<T>(T obj, TimeSpan timeout, MessageListener? listener, int weight, Func<ParsedMessage, bool> handler)
        //{
        //    // TODO either Query<T> or Subscription<T> should be passed here instead of T obj
        //    // That would allow to track the Query/Subscription on the PendingRequest instead of the listener, which allow us to match the pending request in the Converter

        //    var pending = new PendingRequest(ExchangeHelpers.NextId(), handler, timeout, listener);
        //    lock (_messageListeners)
        //    {
        //        _pendingRequests.Add(pending);
        //    }

        //    var sendOk = Send(pending.Id, obj, weight);
        //    if (!sendOk)
        //    {
        //        pending.Fail();
        //        return;
        //    }

        //    while (true)
        //    {
        //        if(!_socket.IsOpen)
        //        {
        //            pending.Fail();
        //            return;
        //        }

        //        if (pending.Completed)
        //            return;

        //        await pending.Event.WaitAsync(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);

        //        if (pending.Completed)
        //            return;
        //    }
        //}

        /// <summary>
        /// Send data over the websocket connection
        /// </summary>
        /// <typeparam name="T">The type of the object to send</typeparam>
        /// <param name="requestId">The request id</param>
        /// <param name="obj">The object to send</param>
        /// <param name="nullValueHandling">How null values should be serialized</param>
        /// <param name="weight">The weight of the message</param>
        public virtual bool Send<T>(int requestId, T obj, int weight, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if(obj is string str)
                return Send(requestId, str, weight);
            else
                return Send(requestId, JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = nullValueHandling }), weight);
        }

        /// <summary>
        /// Send string data over the websocket connection
        /// </summary>
        /// <param name="data">The data to send</param>
        /// <param name="weight">The weight of the message</param>
        /// <param name="requestId">The id of the request</param>
        public virtual bool Send(int requestId, string data, int weight)
        {
            _logger.Log(LogLevel.Trace, $"Socket {SocketId} - msg {requestId} - sending messsage: {data}");
            try
            {
                _socket.Send(requestId, data, weight);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private async Task<CallResult> ProcessReconnectAsync()
        {
            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            bool anySubscriptions = false;
            lock (_listenerLock)
                anySubscriptions = _messageListeners.Any(s => s.UserSubscription);

            if (!anySubscriptions)
            {
                // No need to resubscribe anything
                _logger.Log(LogLevel.Debug, $"Socket {SocketId} Nothing to resubscribe, closing connection");
                _ = _socket.CloseAsync();
                return new CallResult<bool>(true);
            }

            bool anyAuthenticated = false;
            lock (_listenerLock)
                anyAuthenticated = _messageListeners.Any(s => s.Authenticated);

            if (anyAuthenticated)
            {
                // If we reconnected a authenticated connection we need to re-authenticate
                var authResult = await ApiClient.AuthenticateSocketAsync(this).ConfigureAwait(false);
                if (!authResult)
                {
                    _logger.Log(LogLevel.Warning, $"Socket {SocketId} authentication failed on reconnected socket. Disconnecting and reconnecting.");
                    return authResult;
                }

                Authenticated = true;
                _logger.Log(LogLevel.Debug, $"Socket {SocketId} authentication succeeded on reconnected socket.");
            }

            // Get a list of all subscriptions on the socket
            List<Subscription> listenerList = new List<Subscription>();
            lock (_listenerLock)
            {
                // ?
                foreach (var listener in _messageListeners)
                {
                    if (listener != null)
                        listenerList.Add(listener);
                    else
                        listener.Confirmed = true;
                }
            }

            foreach(var listener in listenerList)
            {
                var result = await ApiClient.RevitalizeRequestAsync(listener).ConfigureAwait(false);
                if (!result)
                {
                    _logger.Log(LogLevel.Warning, $"Socket {SocketId} Failed request revitalization: " + result.Error);
                    return result.As<bool>(false);
                }
            }

            // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
            for (var i = 0; i < listenerList.Count; i += ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket)
            {
                if (!_socket.IsOpen)
                    return new CallResult<bool>(new WebError("Socket not connected"));

                var taskList = new List<Task<CallResult>>();
                foreach (var listener in listenerList.Skip(i).Take(ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket))
                    taskList.Add(ApiClient.SubscribeAndWaitAsync(this, listener));

                await Task.WhenAll(taskList).ConfigureAwait(false);
                if (taskList.Any(t => !t.Result.Success))
                    return taskList.First(t => !t.Result.Success).Result;
            }

            foreach (var listener in listenerList)
                listener.Confirmed = true;

            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            _logger.Log(LogLevel.Debug, $"Socket {SocketId} all subscription successfully resubscribed on reconnected socket.");
            return new CallResult<bool>(true);
        }

        internal async Task UnsubscribeAsync(Subscription subscription)
        {
            var unsubscribeRequest = subscription?.GetUnsubRequest();
            if (unsubscribeRequest != null)
            {
                var pendingRequest = PendingRequest.CreateForUnsubRequest(subscription!);
                await SendAndWaitAsync(pendingRequest, 1).ConfigureAwait(false);

                _logger.Log(LogLevel.Information, $"Socket {SocketId} subscription {subscription.Id} unsubscribed");
            }
        }

        internal async Task<CallResult> ResubscribeAsync(Subscription subscription)
        {
            if (!_socket.IsOpen)
                return new CallResult(new UnknownError("Socket is not connected"));

            return await ApiClient.SubscribeAndWaitAsync(this, subscription).ConfigureAwait(false);
        }

        /// <summary>
        /// Status of the socket connection
        /// </summary>
        public enum SocketStatus
        {
            /// <summary>
            /// None/Initial
            /// </summary>
            None,
            /// <summary>
            /// Connected
            /// </summary>
            Connected,
            /// <summary>
            /// Reconnecting
            /// </summary>
            Reconnecting,
            /// <summary>
            /// Resubscribing on reconnected socket
            /// </summary>
            Resubscribing,
            /// <summary>
            /// Closing
            /// </summary>
            Closing,
            /// <summary>
            /// Closed
            /// </summary>
            Closed,
            /// <summary>
            /// Disposed
            /// </summary>
            Disposed
        }
    }
}

