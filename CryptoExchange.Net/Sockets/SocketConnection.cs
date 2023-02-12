using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CryptoExchange.Net.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.Objects;
using System.Net.WebSockets;

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
        public event Action<JToken>? UnhandledMessage;

        /// <summary>
        /// The amount of subscriptions on this connection
        /// </summary>
        public int SubscriptionCount
        {
            get { lock (_subscriptionLock)
                return _subscriptions.Count(h => h.UserSubscription); }
        }

        /// <summary>
        /// Get a copy of the current subscriptions
        /// </summary>
        public SocketSubscription[] Subscriptions
        {
            get
            {
                lock (_subscriptionLock)
                    return _subscriptions.Where(h => h.UserSubscription).ToArray();
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
                    _log.Write(LogLevel.Information, $"Socket {SocketId} Paused activity: " + value);
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
                _log.Write(LogLevel.Debug, $"Socket {SocketId} status changed from {oldStatus} to {_status}");
            }
        }

        private bool _pausedActivity;
        private readonly List<SocketSubscription> _subscriptions;
        private readonly object _subscriptionLock = new();

        private readonly Log _log;

        private readonly List<PendingRequest> _pendingRequests;

        private SocketStatus _status;

        /// <summary>
        /// The underlying websocket
        /// </summary>
        private readonly IWebsocket _socket;

        /// <summary>
        /// New socket connection
        /// </summary>
        /// <param name="log">The logger</param>
        /// <param name="apiClient">The api client</param>
        /// <param name="socket">The socket</param>
        /// <param name="tag"></param>
        public SocketConnection(Log log, SocketApiClient apiClient, IWebsocket socket, string tag)
        {
            this._log = log;
            ApiClient = apiClient;
            Tag = tag;

            _pendingRequests = new List<PendingRequest>();
            _subscriptions = new List<SocketSubscription>();

            _socket = socket;
            _socket.OnMessage += HandleMessage;
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
                foreach (var sub in _subscriptions)
                    sub.Confirmed = false;
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
                foreach (var sub in _subscriptions)
                    sub.Confirmed = false;
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
            lock (_pendingRequests)
            {
                foreach (var pendingRequest in _pendingRequests.ToList())
                {
                    pendingRequest.Fail();
                    _pendingRequests.Remove(pendingRequest);
                }
            }

            var reconnectSuccessful = await ProcessReconnectAsync().ConfigureAwait(false);
            if (!reconnectSuccessful)
            {
                _log.Write(LogLevel.Warning, "Failed reconnect processing, reconnecting again");
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
                _log.Write(LogLevel.Warning, $"Socket {SocketId} error: Websocket error code {wse.WebSocketErrorCode}, details: " + e.ToLogString());
            else
                _log.Write(LogLevel.Warning, $"Socket {SocketId} error: " + e.ToLogString());
        }

        /// <summary>
        /// Process a message received by the socket
        /// </summary>
        /// <param name="data">The received data</param>
        protected virtual void HandleMessage(string data)
        {
            var timestamp = DateTime.UtcNow;
            _log.Write(LogLevel.Trace, $"Socket {SocketId} received data: " + data);
            if (string.IsNullOrEmpty(data)) return;

            var tokenData = data.ToJToken(_log);
            if (tokenData == null)
            {
                data = $"\"{data}\"";
                tokenData = data.ToJToken(_log);
                if (tokenData == null)
                    return;
            }

            var handledResponse = false;

            // Remove any timed out requests
            PendingRequest[] requests;
            lock (_pendingRequests)
            {
                // Remove only timed out requests after 5 minutes have passed so we can still process any 
                // message coming in after the request timeout
                _pendingRequests.RemoveAll(r => r.Completed && DateTime.UtcNow - r.RequestTimestamp > TimeSpan.FromMinutes(5));
                requests = _pendingRequests.ToArray();
            }

            // Check if this message is an answer on any pending requests
            foreach (var pendingRequest in requests)
            {

                if (pendingRequest.CheckData(tokenData))
                {
                    lock (_pendingRequests)
                        _pendingRequests.Remove(pendingRequest);

                    if (pendingRequest.Completed)
                    {
                        // Answer to a timed out request, unsub if it is a subscription request
                        if (pendingRequest.Subscription != null)
                        {
                            _log.Write(LogLevel.Warning, "Received subscription info after request timed out; unsubscribing. Consider increasing the SocketResponseTimout");
                            _ = ApiClient.UnsubscribeAsync(this, pendingRequest.Subscription).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        pendingRequest.Succeed(tokenData);
                    }

                    if (!ApiClient.ContinueOnQueryResponse)
                        return;

                    handledResponse = true;
                    break;
                }
            }

            // Message was not a request response, check data handlers
            var messageEvent = new MessageEvent(this, tokenData, ApiClient.Options.OutputOriginalData ? data : null, timestamp);
            var (handled, userProcessTime, subscription) = HandleData(messageEvent);
            if (!handled && !handledResponse)
            {
                if (!ApiClient.UnhandledMessageExpected)
                    _log.Write(LogLevel.Warning, $"Socket {SocketId} Message not handled: " + tokenData);
                UnhandledMessage?.Invoke(tokenData);
            }

            var total = DateTime.UtcNow - timestamp;
            if (userProcessTime.TotalMilliseconds > 500)
            {
                _log.Write(LogLevel.Debug, $"Socket {SocketId}{(subscription == null ? "" : " subscription " + subscription!.Id)} message processing slow ({(int)total.TotalMilliseconds}ms, {(int)userProcessTime.TotalMilliseconds}ms user code), consider offloading data handling to another thread. " +
                                                "Data from this socket may arrive late or not at all if message processing is continuously slow.");
            }

            _log.Write(LogLevel.Trace, $"Socket {SocketId}{(subscription == null ? "" : " subscription " + subscription!.Id)} message processed in {(int)total.TotalMilliseconds}ms, ({(int)userProcessTime.TotalMilliseconds}ms user code)");
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
        /// <returns></returns>
        public async Task CloseAsync(SocketSubscription subscription)
        {
            lock (_subscriptionLock)
            {
                if (!_subscriptions.Contains(subscription))
                    return;

                subscription.Closed = true;
            }

            if (Status == SocketStatus.Closing || Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            _log.Write(LogLevel.Debug, $"Socket {SocketId} closing subscription {subscription.Id}");
            if (subscription.CancellationTokenRegistration.HasValue)
                subscription.CancellationTokenRegistration.Value.Dispose();

            if (subscription.Confirmed && _socket.IsOpen)
                await ApiClient.UnsubscribeAsync(this, subscription).ConfigureAwait(false);

            bool shouldCloseConnection;
            lock (_subscriptionLock)
            {
                if (Status == SocketStatus.Closing)
                {
                    _log.Write(LogLevel.Debug, $"Socket {SocketId} already closing");
                    return;
                }

                shouldCloseConnection = _subscriptions.All(r => !r.UserSubscription || r.Closed);
                if (shouldCloseConnection)
                    Status = SocketStatus.Closing;
            }

            if (shouldCloseConnection)
            {
                _log.Write(LogLevel.Debug, $"Socket {SocketId} closing as there are no more subscriptions");
                await CloseAsync().ConfigureAwait(false);
            }

            lock (_subscriptionLock)
                _subscriptions.Remove(subscription);
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
        public bool AddSubscription(SocketSubscription subscription)
        {
            lock (_subscriptionLock)
            {
                if (Status != SocketStatus.None && Status != SocketStatus.Connected)
                    return false;

                _subscriptions.Add(subscription);
                if(subscription.UserSubscription)
                    _log.Write(LogLevel.Debug, $"Socket {SocketId} adding new subscription with id {subscription.Id}, total subscriptions on connection: {_subscriptions.Count(s => s.UserSubscription)}");
                return true;
            }
        }

        /// <summary>
        /// Get a subscription on this connection by id
        /// </summary>
        /// <param name="id"></param>
        public SocketSubscription? GetSubscription(int id)
        {
            lock (_subscriptionLock)
                return _subscriptions.SingleOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Get a subscription on this connection by its subscribe request
        /// </summary>
        /// <param name="predicate">Filter for a request</param>
        /// <returns></returns>
        public SocketSubscription? GetSubscriptionByRequest(Func<object?, bool> predicate)
        {
            lock(_subscriptionLock)
                return _subscriptions.SingleOrDefault(s => predicate(s.Request));
        }

        /// <summary>
        /// Process data
        /// </summary>
        /// <param name="messageEvent"></param>
        /// <returns>True if the data was successfully handled</returns>
        private (bool, TimeSpan, SocketSubscription?) HandleData(MessageEvent messageEvent)
        {
            SocketSubscription? currentSubscription = null;
            try
            { 
                var handled = false;
                TimeSpan userCodeDuration = TimeSpan.Zero;

                // Loop the subscriptions to check if any of them signal us that the message is for them
                List<SocketSubscription> subscriptionsCopy;
                lock (_subscriptionLock)
                    subscriptionsCopy = _subscriptions.ToList();

                foreach (var subscription in subscriptionsCopy)
                {
                    currentSubscription = subscription;
                    if (subscription.Request == null)
                    {
                        if (ApiClient.MessageMatchesHandler(this, messageEvent.JsonData, subscription.Identifier!))
                        {
                            handled = true;
                            var userSw = Stopwatch.StartNew();
                            subscription.MessageHandler(messageEvent);
                            userSw.Stop();
                            userCodeDuration = userSw.Elapsed;
                        }
                    }
                    else
                    {
                        if (ApiClient.MessageMatchesHandler(this, messageEvent.JsonData, subscription.Request))
                        {
                            handled = true;
                            messageEvent.JsonData = ApiClient.ProcessTokenData(messageEvent.JsonData);
                            var userSw = Stopwatch.StartNew();
                            subscription.MessageHandler(messageEvent);
                            userSw.Stop();
                            userCodeDuration = userSw.Elapsed;
                        }
                    }
                }
                               
                return (handled, userCodeDuration, currentSubscription);
            }
            catch (Exception ex)
            {
                _log.Write(LogLevel.Error, $"Socket {SocketId} Exception during message processing\r\nException: {ex.ToLogString()}\r\nData: {messageEvent.JsonData}");
                currentSubscription?.InvokeExceptionHandler(ex);
                return (false, TimeSpan.Zero, null);
            }
        }

        /// <summary>
        /// Send data and wait for an answer
        /// </summary>
        /// <typeparam name="T">The data type expected in response</typeparam>
        /// <param name="obj">The object to send</param>
        /// <param name="timeout">The timeout for response</param>
        /// <param name="subscription">Subscription if this is a subscribe request</param>
        /// <param name="handler">The response handler, should return true if the received JToken was the response to the request</param>
        /// <returns></returns>
        public virtual Task SendAndWaitAsync<T>(T obj, TimeSpan timeout, SocketSubscription? subscription, Func<JToken, bool> handler)
        {
            var pending = new PendingRequest(handler, timeout, subscription);
            lock (_pendingRequests)
            {
                _pendingRequests.Add(pending);
            }
            var sendOk = Send(obj);
            if(!sendOk)            
                pending.Fail();            

            return pending.Event.WaitAsync(timeout);
        }

        /// <summary>
        /// Send data over the websocket connection
        /// </summary>
        /// <typeparam name="T">The type of the object to send</typeparam>
        /// <param name="obj">The object to send</param>
        /// <param name="nullValueHandling">How null values should be serialized</param>
        public virtual bool Send<T>(T obj, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if(obj is string str)
                return Send(str);
            else
                return Send(JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = nullValueHandling }));
        }

        /// <summary>
        /// Send string data over the websocket connection
        /// </summary>
        /// <param name="data">The data to send</param>
        public virtual bool Send(string data)
        {
            _log.Write(LogLevel.Trace, $"Socket {SocketId} sending data: {data}");
            try
            {
                _socket.Send(data);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private async Task<CallResult<bool>> ProcessReconnectAsync()
        {
            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            bool anySubscriptions = false;
            lock (_subscriptionLock)
                anySubscriptions = _subscriptions.Any(s => s.UserSubscription);

            if (!anySubscriptions)
            {
                // No need to resubscribe anything
                _log.Write(LogLevel.Debug, $"Socket {SocketId} Nothing to resubscribe, closing connection");
                _ = _socket.CloseAsync();
                return new CallResult<bool>(true);
            }

            if (_subscriptions.Any(s => s.Authenticated))
            {
                // If we reconnected a authenticated connection we need to re-authenticate
                var authResult = await ApiClient.AuthenticateSocketAsync(this).ConfigureAwait(false);
                if (!authResult)
                {
                    _log.Write(LogLevel.Warning, $"Socket {SocketId} authentication failed on reconnected socket. Disconnecting and reconnecting.");
                    return authResult;
                }

                Authenticated = true;
                _log.Write(LogLevel.Debug, $"Socket {SocketId} authentication succeeded on reconnected socket.");
            }

            // Get a list of all subscriptions on the socket
            List<SocketSubscription> subscriptionList = new List<SocketSubscription>();
            lock (_subscriptionLock)
            {
                foreach (var subscription in _subscriptions)
                {
                    if (subscription.Request != null)
                        subscriptionList.Add(subscription);
                    else
                        subscription.Confirmed = true;
                }
            }

            foreach(var subscription in subscriptionList.Where(s => s.Request != null))
            {
                var result = await ApiClient.RevitalizeRequestAsync(subscription.Request!).ConfigureAwait(false);
                if (!result)
                {
                    _log.Write(LogLevel.Warning, "Failed request revitalization: " + result.Error);
                    return result.As<bool>(false);
                }
            }

            // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
            for (var i = 0; i < subscriptionList.Count; i += ApiClient.Options.MaxConcurrentResubscriptionsPerSocket)
            {
                if (!_socket.IsOpen)
                    return new CallResult<bool>(new WebError("Socket not connected"));

                var taskList = new List<Task<CallResult<bool>>>();
                foreach (var subscription in subscriptionList.Skip(i).Take(ApiClient.Options.MaxConcurrentResubscriptionsPerSocket))
                    taskList.Add(ApiClient.SubscribeAndWaitAsync(this, subscription.Request!, subscription));

                await Task.WhenAll(taskList).ConfigureAwait(false);
                if (taskList.Any(t => !t.Result.Success))
                    return taskList.First(t => !t.Result.Success).Result;
            }

            foreach (var subscription in subscriptionList)
                subscription.Confirmed = true;

            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            _log.Write(LogLevel.Debug, $"Socket {SocketId} all subscription successfully resubscribed on reconnected socket.");
            return new CallResult<bool>(true);
        }

        internal async Task UnsubscribeAsync(SocketSubscription socketSubscription)
        {
            await ApiClient.UnsubscribeAsync(this, socketSubscription).ConfigureAwait(false);
        }

        internal async Task<CallResult<bool>> ResubscribeAsync(SocketSubscription socketSubscription)
        {
            if (!_socket.IsOpen)
                return new CallResult<bool>(new UnknownError("Socket is not connected"));

            return await ApiClient.SubscribeAndWaitAsync(this, socketSubscription.Request!, socketSubscription).ConfigureAwait(false);
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
