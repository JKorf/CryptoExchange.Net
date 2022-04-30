using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.Objects;

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
            get { lock (subscriptionLock)
                return subscriptions.Count(h => h.UserSubscription); }
        }

        /// <summary>
        /// If the connection has been authenticated
        /// </summary>
        public bool Authenticated { get; set; }

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
        public Uri Uri => _socket.Uri;

        /// <summary>
        /// The API client the connection is for
        /// </summary>
        public SocketApiClient ApiClient { get; set; }

        /// <summary>
        /// If the socket should be reconnected upon closing
        /// </summary>
        public bool ShouldReconnect { get; set; }

        /// <summary>
        /// Current reconnect try, reset when a successful connection is made
        /// </summary>
        public int ReconnectTry { get; set; }

        /// <summary>
        /// Current resubscribe try, reset when a successful connection is made
        /// </summary>
        public int ResubscribeTry { get; set; }

        /// <summary>
        /// Time of disconnecting
        /// </summary>
        public DateTime? DisconnectTime { get; set; }

        /// <summary>
        /// Tag for identificaion
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// If activity is paused
        /// </summary>
        public bool PausedActivity
        {
            get => pausedActivity;
            set
            {
                if (pausedActivity != value)
                {
                    pausedActivity = value;
                    log.Write(LogLevel.Information, $"Socket {SocketId} Paused activity: " + value);
                    if(pausedActivity) ActivityPaused?.Invoke();
                    else ActivityUnpaused?.Invoke();
                }
            }
        }

        private bool pausedActivity;
        private readonly List<SocketSubscription> subscriptions;
        private readonly object subscriptionLock = new();

        private bool lostTriggered;
        private readonly Log log;
        private readonly BaseSocketClient socketClient;

        private readonly List<PendingRequest> pendingRequests;
        private Task? _socketProcessReconnectTask;

        /// <summary>
        /// The underlying websocket
        /// </summary>
        private readonly IWebsocket _socket;

        /// <summary>
        /// New socket connection
        /// </summary>
        /// <param name="client">The socket client</param>
        /// <param name="apiClient">The api client</param>
        /// <param name="socket">The socket</param>
        public SocketConnection(BaseSocketClient client, SocketApiClient apiClient, IWebsocket socket)
        {
            log = client.log;
            socketClient = client;
            ApiClient = apiClient;

            pendingRequests = new List<PendingRequest>();

            subscriptions = new List<SocketSubscription>();
            _socket = socket;

            _socket.Timeout = client.ClientOptions.SocketNoDataTimeout;
            _socket.OnMessage += ProcessMessage;
            _socket.OnOpen += SocketOnOpen;
        }
        
        /// <summary>
        /// Connect the websocket and start processing
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync()
        {
            var connected = await _socket.ConnectAsync().ConfigureAwait(false);
            if (connected)
                StartProcessingTask();

            return connected;
        }

        /// <summary>
        /// Retrieve the underlying socket
        /// </summary>
        /// <returns></returns>
        public IWebsocket GetSocket()
        {
            return _socket;
        }

        /// <summary>
        /// Trigger a reconnect of the socket connection
        /// </summary>
        /// <returns></returns>
        public async Task TriggerReconnectAsync()
        {
            await _socket.CloseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            ShouldReconnect = false;
            if (socketClient.socketConnections.ContainsKey(SocketId))
                socketClient.socketConnections.TryRemove(SocketId, out _);

            lock (subscriptionLock)
            {
                foreach (var subscription in subscriptions)
                {
                    if (subscription.CancellationTokenRegistration.HasValue)
                        subscription.CancellationTokenRegistration.Value.Dispose();
                }
            }
            await _socket.CloseAsync().ConfigureAwait(false);

            if (_socketProcessReconnectTask != null)
                await _socketProcessReconnectTask.ConfigureAwait(false);

            _socket.Dispose();
        }

        /// <summary>
        /// Close a subscription on this connection. If all subscriptions on this connection are closed the connection gets closed as well
        /// </summary>
        /// <param name="subscription">Subscription to close</param>
        /// <returns></returns>
        public async Task CloseAsync(SocketSubscription subscription)
        {
            if (!_socket.IsOpen)
                return;

            if (subscription.CancellationTokenRegistration.HasValue)
                subscription.CancellationTokenRegistration.Value.Dispose();

            if (subscription.Confirmed)
                await socketClient.UnsubscribeAsync(this, subscription).ConfigureAwait(false);

            bool shouldCloseConnection;
            lock (subscriptionLock)
                shouldCloseConnection = !subscriptions.Any(r => r.UserSubscription && subscription != r);

            if (shouldCloseConnection)
                await CloseAsync().ConfigureAwait(false);

            lock (subscriptionLock)
                subscriptions.Remove(subscription);
        }

        private void StartProcessingTask()
        {
            log.Write(LogLevel.Trace, "Starting processing task");
            _socketProcessReconnectTask = Task.Run(async () =>
            {
                await _socket.ProcessAsync().ConfigureAwait(false);
                await ReconnectAsync().ConfigureAwait(false);
                log.Write(LogLevel.Trace, "Processing task finished");
            });
        }

        private async Task ReconnectAsync()
        {
            // Fail all pending requests
            lock (pendingRequests)
            {
                foreach (var pendingRequest in pendingRequests.ToList())
                {
                    pendingRequest.Fail();
                    pendingRequests.Remove(pendingRequest);
                }
            }

            if (socketClient.ClientOptions.AutoReconnect && ShouldReconnect)
            {
                // Should reconnect
                DisconnectTime = DateTime.UtcNow;
                log.Write(LogLevel.Warning, $"Socket {SocketId} Connection lost, will try to reconnect");
                if (!lostTriggered)
                {
                    lostTriggered = true;
                    ConnectionLost?.Invoke();
                }

                while (ShouldReconnect)
                {
                    if (ReconnectTry > 0)
                    {
                        // Wait a bit before attempting reconnect
                        await Task.Delay(socketClient.ClientOptions.ReconnectInterval).ConfigureAwait(false);
                    }

                    if (!ShouldReconnect)
                    {
                        // Should reconnect changed to false while waiting to reconnect
                        return;
                    }

                    _socket.Reset();
                    if (!await _socket.ConnectAsync().ConfigureAwait(false))
                    {
                        // Reconnect failed
                        ReconnectTry++;
                        ResubscribeTry = 0;
                        if (socketClient.ClientOptions.MaxReconnectTries != null
                        && ReconnectTry >= socketClient.ClientOptions.MaxReconnectTries)
                        {
                            log.Write(LogLevel.Warning, $"Socket {SocketId} failed to reconnect after {ReconnectTry} tries, closing");
                            ShouldReconnect = false;

                            if (socketClient.socketConnections.ContainsKey(SocketId))
                                socketClient.socketConnections.TryRemove(SocketId, out _);

                            _ = Task.Run(() => ConnectionClosed?.Invoke());
                            // Reached max tries, break loop and leave connection closed
                            break;
                        }

                        // Continue to try again
                        log.Write(LogLevel.Debug, $"Socket {SocketId} failed to reconnect{(socketClient.ClientOptions.MaxReconnectTries != null ? $", try {ReconnectTry}/{socketClient.ClientOptions.MaxReconnectTries}" : "")}, will try again in {socketClient.ClientOptions.ReconnectInterval}");
                        continue;
                    }

                    // Successfully reconnected, start processing
                    StartProcessingTask();

                    ReconnectTry = 0;
                    var time = DisconnectTime;
                    DisconnectTime = null;

                    log.Write(LogLevel.Information, $"Socket {SocketId} reconnected after {DateTime.UtcNow - time}");

                    var reconnectResult = await ProcessReconnectAsync().ConfigureAwait(false);
                    if (!reconnectResult)
                    {
                        // Failed to resubscribe everything
                        ResubscribeTry++;
                        DisconnectTime = time;

                        if (socketClient.ClientOptions.MaxResubscribeTries != null &&
                        ResubscribeTry >= socketClient.ClientOptions.MaxResubscribeTries)
                        {
                            log.Write(LogLevel.Warning, $"Socket {SocketId} failed to resubscribe after {ResubscribeTry} tries, closing. Last resubscription error: {reconnectResult.Error}");
                            ShouldReconnect = false;

                            if (socketClient.socketConnections.ContainsKey(SocketId))
                                socketClient.socketConnections.TryRemove(SocketId, out _);

                            _ = Task.Run(() => ConnectionClosed?.Invoke());
                        }
                        else
                            log.Write(LogLevel.Debug, $"Socket {SocketId} resubscribing all subscriptions failed on reconnected socket{(socketClient.ClientOptions.MaxResubscribeTries != null ? $", try {ResubscribeTry}/{socketClient.ClientOptions.MaxResubscribeTries}" : "")}. Error: {reconnectResult.Error}. Disconnecting and reconnecting.");

                        // Failed resubscribe, close socket if it is still open
                        if (_socket.IsOpen)
                            await _socket.CloseAsync().ConfigureAwait(false);
                        else
                            DisconnectTime = DateTime.UtcNow;

                        // Break out of the loop, the new processing task should reconnect again
                        break;
                    }
                    else
                    {
                        // Succesfully reconnected
                        log.Write(LogLevel.Information, $"Socket {SocketId} data connection restored.");
                        ResubscribeTry = 0;
                        if (lostTriggered)
                        {
                            lostTriggered = false;
                            _ = Task.Run(() => ConnectionRestored?.Invoke(time.HasValue ? DateTime.UtcNow - time.Value : TimeSpan.FromSeconds(0))).ConfigureAwait(false);
                        }

                        break;
                    }
                }
            }
            else
            {
                if (!socketClient.ClientOptions.AutoReconnect && ShouldReconnect)
                    _ = Task.Run(() => ConnectionClosed?.Invoke());

                // No reconnecting needed
                log.Write(LogLevel.Information, $"Socket {SocketId} closed");
                if (socketClient.socketConnections.ContainsKey(SocketId))
                    socketClient.socketConnections.TryRemove(SocketId, out _);
            }
        }

        /// <summary>
        /// Dispose the connection
        /// </summary>
        public void Dispose()
        {
            _socket.Dispose();
        }

        /// <summary>
        /// Process a message received by the socket
        /// </summary>
        /// <param name="data">The received data</param>
        private void ProcessMessage(string data)
        {
            var timestamp = DateTime.UtcNow;
            log.Write(LogLevel.Trace, $"Socket {SocketId} received data: " + data);
            if (string.IsNullOrEmpty(data)) return;

            var tokenData = data.ToJToken(log);
            if (tokenData == null)
            {
                data = $"\"{data}\"";
                tokenData = data.ToJToken(log);
                if (tokenData == null)
                    return;
            }

            var handledResponse = false;
            PendingRequest[] requests;
            lock(pendingRequests)			
                requests = pendingRequests.ToArray();

            // Remove any timed out requests
            foreach (var request in requests.Where(r => r.Completed))
            {
                lock (pendingRequests)
                    pendingRequests.Remove(request);
            }

            // Check if this message is an answer on any pending requests
            foreach (var pendingRequest in requests)
            {
                if (pendingRequest.CheckData(tokenData))
                {
                    lock (pendingRequests)                    
                        pendingRequests.Remove(pendingRequest);

                    if (!socketClient.ContinueOnQueryResponse)
                        return;

                    handledResponse = true;
                    break;
                }
            }

            // Message was not a request response, check data handlers
            var messageEvent = new MessageEvent(this, tokenData, socketClient.ClientOptions.OutputOriginalData ? data: null, timestamp);
            var (handled, userProcessTime) = HandleData(messageEvent);
            if (!handled && !handledResponse)
            {
                if (!socketClient.UnhandledMessageExpected)
                    log.Write(LogLevel.Warning, $"Socket {SocketId} Message not handled: " + tokenData);
                UnhandledMessage?.Invoke(tokenData);
            }

            var total = DateTime.UtcNow - timestamp;
            if (userProcessTime.TotalMilliseconds > 500)
                log.Write(LogLevel.Debug, $"Socket {SocketId} message processing slow ({(int)total.TotalMilliseconds}ms), consider offloading data handling to another thread. " +
                                                "Data from this socket may arrive late or not at all if message processing is continuously slow.");
            
            log.Write(LogLevel.Trace, $"Socket {SocketId} message processed in {(int)total.TotalMilliseconds}ms, ({(int)userProcessTime.TotalMilliseconds}ms user code)");
        }

        /// <summary>
        /// Add a subscription to this connection
        /// </summary>
        /// <param name="subscription"></param>
        public void AddSubscription(SocketSubscription subscription)
        {
            lock(subscriptionLock)
                subscriptions.Add(subscription);
        }

        /// <summary>
        /// Get a subscription on this connection by id
        /// </summary>
        /// <param name="id"></param>
        public SocketSubscription? GetSubscription(int id)
        {
            lock (subscriptionLock)
                return subscriptions.SingleOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Get a subscription on this connection by its subscribe request
        /// </summary>
        /// <param name="predicate">Filter for a request</param>
        /// <returns></returns>
        public SocketSubscription? GetSubscriptionByRequest(Func<object?, bool> predicate)
        {
            lock(subscriptionLock)
                return subscriptions.SingleOrDefault(s => predicate(s.Request));
        }

        /// <summary>
        /// Process data
        /// </summary>
        /// <param name="messageEvent"></param>
        /// <returns>True if the data was successfully handled</returns>
        private (bool, TimeSpan) HandleData(MessageEvent messageEvent)
        {
            SocketSubscription? currentSubscription = null;
            try
            { 
                var handled = false;
                TimeSpan userCodeDuration = TimeSpan.Zero;

                // Loop the subscriptions to check if any of them signal us that the message is for them
                List<SocketSubscription> subscriptionsCopy;
                lock (subscriptionLock)
                    subscriptionsCopy = subscriptions.ToList();

                foreach (var subscription in subscriptionsCopy)
                {
                    currentSubscription = subscription;
                    if (subscription.Request == null)
                    {
                        if (socketClient.MessageMatchesHandler(this, messageEvent.JsonData, subscription.Identifier!))
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
                        if (socketClient.MessageMatchesHandler(this, messageEvent.JsonData, subscription.Request))
                        {
                            handled = true;
                            messageEvent.JsonData = socketClient.ProcessTokenData(messageEvent.JsonData);
                            var userSw = Stopwatch.StartNew();
                            subscription.MessageHandler(messageEvent);
                            userSw.Stop();
                            userCodeDuration = userSw.Elapsed;
                        }
                    }
                }
                               
                return (handled, userCodeDuration);
            }
            catch (Exception ex)
            {
                log.Write(LogLevel.Error, $"Socket {SocketId} Exception during message processing\r\nException: {ex.ToLogString()}\r\nData: {messageEvent.JsonData}");
                currentSubscription?.InvokeExceptionHandler(ex);
                return (false, TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Send data and wait for an answer
        /// </summary>
        /// <typeparam name="T">The data type expected in response</typeparam>
        /// <param name="obj">The object to send</param>
        /// <param name="timeout">The timeout for response</param>
        /// <param name="handler">The response handler, should return true if the received JToken was the response to the request</param>
        /// <returns></returns>
        public virtual Task SendAndWaitAsync<T>(T obj, TimeSpan timeout, Func<JToken, bool> handler)
        {
            var pending = new PendingRequest(handler, timeout);
            lock (pendingRequests)
            {
                pendingRequests.Add(pending);
            }
            Send(obj);
            return pending.Event.WaitAsync(timeout);
        }

        /// <summary>
        /// Send data over the websocket connection
        /// </summary>
        /// <typeparam name="T">The type of the object to send</typeparam>
        /// <param name="obj">The object to send</param>
        /// <param name="nullValueHandling">How null values should be serialized</param>
        public virtual void Send<T>(T obj, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if(obj is string str)
                Send(str);
            else
                Send(JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = nullValueHandling }));
        }

        /// <summary>
        /// Send string data over the websocket connection
        /// </summary>
        /// <param name="data">The data to send</param>
        public virtual void Send(string data)
        {
            log.Write(LogLevel.Trace, $"Socket {SocketId} sending data: {data}");
            _socket.Send(data);
        }

        /// <summary>
        /// Handler for a socket opening
        /// </summary>
        protected virtual void SocketOnOpen()
        {
            ReconnectTry = 0;
            PausedActivity = false;
        }

        private async Task<CallResult<bool>> ProcessReconnectAsync()
        {
            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            if (Authenticated)
            {
                // If we reconnected a authenticated connection we need to re-authenticate
                var authResult = await socketClient.AuthenticateSocketAsync(this).ConfigureAwait(false);
                if (!authResult)
                {
                    log.Write(LogLevel.Warning, $"Socket {SocketId} authentication failed on reconnected socket. Disconnecting and reconnecting.");
                    return authResult;
                }

                log.Write(LogLevel.Debug, $"Socket {SocketId} authentication succeeded on reconnected socket.");
            }

            // Get a list of all subscriptions on the socket
            List<SocketSubscription> subscriptionList;
            lock (subscriptionLock)
                subscriptionList = subscriptions.Where(h => h.Request != null).ToList();

            // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
            for (var i = 0; i < subscriptionList.Count; i += socketClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket)
            {
                if (!_socket.IsOpen)
                    return new CallResult<bool>(new WebError("Socket not connected"));

                var taskList = new List<Task<CallResult<bool>>>();
                foreach (var subscription in subscriptionList.Skip(i).Take(socketClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket))
                    taskList.Add(socketClient.SubscribeAndWaitAsync(this, subscription.Request!, subscription));                

                await Task.WhenAll(taskList).ConfigureAwait(false);
                if (taskList.Any(t => !t.Result.Success))
                    return taskList.First(t => !t.Result.Success).Result;
            }

            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            log.Write(LogLevel.Debug, $"Socket {SocketId} all subscription successfully resubscribed on reconnected socket.");
            return new CallResult<bool>(true);
        }

        internal async Task UnsubscribeAsync(SocketSubscription socketSubscription)
        {
            await socketClient.UnsubscribeAsync(this, socketSubscription).ConfigureAwait(false);
        }

        internal async Task<CallResult<bool>> ResubscribeAsync(SocketSubscription socketSubscription)
        {
            if (!_socket.IsOpen)
                return new CallResult<bool>(new UnknownError("Socket is not connected"));

            return await socketClient.SubscribeAndWaitAsync(this, socketSubscription.Request!, socketSubscription).ConfigureAwait(false);
        }
    }
}
