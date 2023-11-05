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
using System.Text;

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
        public event Action<BaseParsedMessage>? UnhandledMessage;

        /// <summary>
        /// Unparsed message event
        /// </summary>
        public event Action<byte[]>? UnparsedMessage;

        /// <summary>
        /// The amount of subscriptions on this connection
        /// </summary>
        public int UserSubscriptionCount
        {
            get { lock (_subscriptionLock)
                return _messageIdentifierSubscriptions.Values.Count(h => h.UserSubscription); }
        }

        /// <summary>
        /// Get a copy of the current message subscriptions
        /// </summary>
        public Subscription[] Subscriptions
        {
            get
            {
                lock (_subscriptionLock)
                    return _messageIdentifierSubscriptions.Values.Where(h => h.UserSubscription).ToArray();
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
        private readonly List<Subscription> _subscriptions;
        private readonly Dictionary<string, Subscription> _messageIdentifierSubscriptions;

        private readonly object _subscriptionLock = new();
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
            _subscriptions = new List<Subscription>();
            _messageIdentifierSubscriptions = new Dictionary<string, Subscription>();

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
            lock(_subscriptionLock)
            {
                foreach (var subscription in _subscriptions)
                    subscription.Confirmed = false;
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
            lock (_subscriptionLock)
            {
                foreach (var subscription in _subscriptions)
                    subscription.Confirmed = false;
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
            lock (_subscriptions)
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

            List<Subscription> subscriptions;
            lock (_subscriptionLock)
                subscriptions = _subscriptions.OrderByDescending(x => !x.UserSubscription).ToList();

            var result = ApiClient.StreamConverter.ReadJson(stream, _pendingRequests, subscriptions, ApiClient.ApiOptions.OutputOriginalData ?? ApiClient.ClientOptions.OutputOriginalData);
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

            if (!result.Parsed)
            {
                _logger.LogWarning("Message not matched to type");
                return;
            }

            // TODO lock
            if (_messageIdentifierSubscriptions.TryGetValue(result.Identifier.ToLowerInvariant(), out var idSubscription))
            {
                // Matched based on identifier
                var userSw = Stopwatch.StartNew();
                var dataEvent = new DataEvent<BaseParsedMessage>(result, null, result.OriginalData, DateTime.UtcNow, null);
                await idSubscription.HandleEventAsync(dataEvent).ConfigureAwait(false);
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
                        // Answer to a timed out request
                        _logger.Log(LogLevel.Warning, $"Socket {SocketId} Received after request timeout. Consider increasing the RequestTimeout");
                    }
                    else
                    {
                        _logger.Log(LogLevel.Trace, $"Socket {SocketId} - msg {pendingRequest.Id} - received data matched to pending request");
                        pendingRequest.ProcessAsync(result);
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

            lock (_subscriptionLock)
            {
                foreach (var subscription in _subscriptions)
                {
                    if (subscription.CancellationTokenRegistration.HasValue)
                        subscription.CancellationTokenRegistration.Value.Dispose();
                }
            }

            await _socket.CloseAsync().ConfigureAwait(false);
            _socket.Dispose();
        }

        /// <summary>
        /// Close a subscription on this connection. If all subscriptions on this connection are closed the connection gets closed as well
        /// </summary>
        /// <param name="subscription">Subscription to close</param>
        /// <param name="unsubEvenIfNotConfirmed">Whether to send an unsub request even if the subscription wasn't confirmed</param>
        /// <returns></returns>
        public async Task CloseAsync(Subscription subscription, bool unsubEvenIfNotConfirmed = false)
        {
            lock (_subscriptionLock)
            {
                if (!_subscriptions.Contains(subscription))
                    return;

                subscription.Closed = true;
            }

            if (Status == SocketStatus.Closing || Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            _logger.Log(LogLevel.Debug, $"Socket {SocketId} closing subscription {subscription.Id}");
            if (subscription.CancellationTokenRegistration.HasValue)
                subscription.CancellationTokenRegistration.Value.Dispose();

            if ((unsubEvenIfNotConfirmed || subscription.Confirmed) && _socket.IsOpen)
                await UnsubscribeAsync(subscription).ConfigureAwait(false);

            bool shouldCloseConnection;
            lock (_subscriptionLock)
            {
                if (Status == SocketStatus.Closing)
                {
                    _logger.Log(LogLevel.Debug, $"Socket {SocketId} already closing");
                    return;
                }

                shouldCloseConnection = _messageIdentifierSubscriptions.All(r => !r.Value.UserSubscription || r.Value.Closed);
                if (shouldCloseConnection)
                    Status = SocketStatus.Closing;
            }

            if (shouldCloseConnection)
            {
                _logger.Log(LogLevel.Debug, $"Socket {SocketId} closing as there are no more subscriptions");
                await CloseAsync().ConfigureAwait(false);
            }

            lock (_subscriptionLock)
            {
                _subscriptions.Remove(subscription);
                foreach (var id in subscription.Identifiers)
                    _messageIdentifierSubscriptions.Remove(id);
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
        /// Add a subscription to this connection
        /// </summary>
        /// <param name="subscription"></param>
        public bool AddSubscription(Subscription subscription)
        {
            lock (_subscriptionLock)
            {
                if (Status != SocketStatus.None && Status != SocketStatus.Connected)
                    return false;

                _subscriptions.Add(subscription);
                if (subscription.Identifiers != null)
                {
                    foreach (var id in subscription.Identifiers)
                        _messageIdentifierSubscriptions.Add(id.ToLowerInvariant(), subscription);
                }

                if (subscription.UserSubscription)
                    _logger.Log(LogLevel.Debug, $"Socket {SocketId} adding new subscription with id {subscription.Id}, total subscriptions on connection: {_subscriptions.Count(s => s.UserSubscription)}");
                return true;
            }
        }

        /// <summary>
        /// Get a subscription on this connection by id
        /// </summary>
        /// <param name="id"></param>
        public Subscription? GetSubscription(int id)
        {
            lock (_subscriptionLock)
                return _subscriptions.SingleOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Get a subscription on this connection by its subscribe request
        /// </summary>
        /// <param name="predicate">Filter for a request</param>
        /// <returns></returns>
        public Subscription? GetSubscriptionByRequest(Func<object?, bool> predicate)
        {
            lock(_subscriptionLock)
                return _subscriptions.SingleOrDefault(s => predicate(s));
        }

        /// <summary>
        /// Send a query request and wait for an answer
        /// </summary>
        /// <param name="query">Query to send</param>
        /// <returns></returns>
        public virtual async Task<CallResult> SendAndWaitQueryAsync(BaseQuery query)
        {
            var pendingRequest = query.CreatePendingRequest();
            await SendAndWaitAsync(pendingRequest, query.Weight).ConfigureAwait(false);
            return pendingRequest.Result!;
        }

        /// <summary>
        /// Send a query request and wait for an answer
        /// </summary>
        /// <typeparam name="T">Query response type</typeparam>
        /// <param name="query">Query to send</param>
        /// <returns></returns>
        public virtual async Task<CallResult<T>> SendAndWaitQueryAsync<T>(Query<T> query)
        {
            var pendingRequest = PendingRequest<T>.CreateForQuery(query);
            await SendAndWaitAsync(pendingRequest, query.Weight).ConfigureAwait(false);
            return pendingRequest.TypedResult!;
        }

        private async Task SendAndWaitAsync(BasePendingRequest pending, int weight)
        {
            lock (_subscriptions)
                _pendingRequests.Add(pending);

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

                await pending.WaitAsync(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);

                if (pending.Completed)
                    return;
            }
        }

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
            lock (_subscriptionLock)
                anySubscriptions = _subscriptions.Any(s => s.UserSubscription);

            if (!anySubscriptions)
            {
                // No need to resubscribe anything
                _logger.Log(LogLevel.Debug, $"Socket {SocketId} Nothing to resubscribe, closing connection");
                _ = _socket.CloseAsync();
                return new CallResult<bool>(true);
            }

            bool anyAuthenticated = false;
            lock (_subscriptionLock)
                anyAuthenticated = _subscriptions.Any(s => s.Authenticated);

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
            List<Subscription> subList = new List<Subscription>();
            lock (_subscriptionLock)
                subList = _subscriptions.ToList();

            foreach(var subscription in subList)
            {
                var result = await ApiClient.RevitalizeRequestAsync(subscription).ConfigureAwait(false);
                if (!result)
                {
                    _logger.Log(LogLevel.Warning, $"Socket {SocketId} Failed request revitalization: " + result.Error);
                    return result.As<bool>(false);
                }
            }

            // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
            for (var i = 0; i < subList.Count; i += ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket)
            {
                if (!_socket.IsOpen)
                    return new CallResult<bool>(new WebError("Socket not connected"));

                var taskList = new List<Task<CallResult>>();
                foreach (var subscription in subList.Skip(i).Take(ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket))
                {
                    var subQuery = subscription.GetSubQuery();
                    if (subQuery == null)
                        continue;

                    taskList.Add(SendAndWaitQueryAsync(subQuery));
                }

                await Task.WhenAll(taskList).ConfigureAwait(false);
                if (taskList.Any(t => !t.Result.Success))
                    return taskList.First(t => !t.Result.Success).Result;
            }

            foreach (var subscription in subList)
                subscription.Confirmed = true;

            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            _logger.Log(LogLevel.Debug, $"Socket {SocketId} all subscription successfully resubscribed on reconnected socket.");
            return new CallResult<bool>(true);
        }

        internal async Task UnsubscribeAsync(Subscription subscription)
        {
            var unsubscribeRequest = subscription.GetUnsubQuery();
            if (unsubscribeRequest == null)
                return;

            await SendAndWaitQueryAsync(unsubscribeRequest).ConfigureAwait(false);
            _logger.Log(LogLevel.Information, $"Socket {SocketId} subscription {subscription!.Id} unsubscribed");
        }

        internal async Task<CallResult> ResubscribeAsync(Subscription subscription)
        {
            if (!_socket.IsOpen)
                return new CallResult(new UnknownError("Socket is not connected"));

            var subQuery = subscription.GetSubQuery();
            if (subQuery == null)
                return new CallResult(null);

            return await SendAndWaitQueryAsync(subQuery).ConfigureAwait(false);
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

