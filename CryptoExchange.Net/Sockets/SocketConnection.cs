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
    /// Socket connecting
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
        /// Connecting closed event
        /// </summary>
        public event Action? Closed;
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
        /// If connection is authenticated
        /// </summary>
        public bool Authenticated { get; set; }
        /// <summary>
        /// If connection is made
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// The underlying socket
        /// </summary>
        public IWebsocket Socket { get; set; }
        /// <summary>
        /// If the socket should be reconnected upon closing
        /// </summary>
        public bool ShouldReconnect { get; set; }
        /// <summary>
        /// Current reconnect try
        /// </summary>
        public int ReconnectTry { get; set; }
        /// <summary>
        /// Current resubscribe try
        /// </summary>
        public int ResubscribeTry { get; set; }
        /// <summary>
        /// Time of disconnecting
        /// </summary>
        public DateTime? DisconnectTime { get; set; }

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
                    log.Write(LogLevel.Debug, $"Socket {Socket.Id} Paused activity: " + value);
                    if(pausedActivity) ActivityPaused?.Invoke();
                    else ActivityUnpaused?.Invoke();
                }
            }
        }

        private bool pausedActivity;
        private readonly List<SocketSubscription> subscriptions;
        private readonly object subscriptionLock = new object();

        private bool lostTriggered;
        private readonly Log log;
        private readonly SocketClient socketClient;

        private readonly List<PendingRequest> pendingRequests;

        /// <summary>
        /// New socket connection
        /// </summary>
        /// <param name="client">The socket client</param>
        /// <param name="socket">The socket</param>
        public SocketConnection(SocketClient client, IWebsocket socket)
        {
            log = client.log;
            socketClient = client;

            pendingRequests = new List<PendingRequest>();

            subscriptions = new List<SocketSubscription>();
            Socket = socket;

            Socket.Timeout = client.SocketNoDataTimeout;
            Socket.OnMessage += ProcessMessage;
            Socket.OnClose += SocketOnClose;
            Socket.OnOpen += SocketOnOpen;
        }
        
        /// <summary>
        /// Process a message received by the socket
        /// </summary>
        /// <param name="data"></param>
        private void ProcessMessage(string data)
        {
            var timestamp = DateTime.UtcNow;
            log.Write(LogLevel.Trace, $"Socket {Socket.Id} received data: " + data);
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
            var messageEvent = new MessageEvent(this, tokenData, socketClient.OutputOriginalData ? data: null, timestamp);
            if (!HandleData(messageEvent) && !handledResponse)
            {
                if (!socketClient.UnhandledMessageExpected)
                    log.Write(LogLevel.Warning, $"Socket {Socket.Id} Message not handled: " + tokenData);
                UnhandledMessage?.Invoke(tokenData);
            }
        }

        /// <summary>
        /// Add subscription to this connection
        /// </summary>
        /// <param name="subscription"></param>
        public void AddSubscription(SocketSubscription subscription)
        {
            lock(subscriptionLock)
                subscriptions.Add(subscription);
        }

        /// <summary>
        /// Get a subscription on this connection
        /// </summary>
        /// <param name="id"></param>
        public SocketSubscription GetSubscription(int id)
        {
            lock (subscriptionLock)
                return subscriptions.SingleOrDefault(s => s.Id == id);
        }

        private bool HandleData(MessageEvent messageEvent)
        {
            SocketSubscription? currentSubscription = null;
            try
            { 
                var handled = false;
                var sw = Stopwatch.StartNew();

                // Loop the subscriptions to check if any of them signal us that the message is for them
                List<SocketSubscription> subscriptionsCopy;
                lock (subscriptionLock)
                    subscriptionsCopy = subscriptions.ToList();

                foreach (var subscription in subscriptionsCopy)
                {
                    currentSubscription = subscription;
                    if (subscription.Request == null)
                    {
                        if (socketClient.MessageMatchesHandler(messageEvent.JsonData, subscription.Identifier!))
                        {
                            handled = true;
                            subscription.MessageHandler(messageEvent);
                        }
                    }
                    else
                    {
                        if (socketClient.MessageMatchesHandler(messageEvent.JsonData, subscription.Request))
                        {
                            handled = true;
                            messageEvent.JsonData = socketClient.ProcessTokenData(messageEvent.JsonData);
                            subscription.MessageHandler(messageEvent);
                        }
                    }
                }
                
                sw.Stop();
                if (sw.ElapsedMilliseconds > 500)
                    log.Write(LogLevel.Warning, $"Socket {Socket.Id} message processing slow ({sw.ElapsedMilliseconds}ms), consider offloading data handling to another thread. " +
                                                    "Data from this socket may arrive late or not at all if message processing is continuously slow.");
                else
                    log.Write(LogLevel.Trace, $"Socket {Socket.Id} message processed in {sw.ElapsedMilliseconds}ms");
                return handled;
            }
            catch (Exception ex)
            {
                log.Write(LogLevel.Error, $"Socket {Socket.Id} Exception during message processing\r\nException: {ex.ToLogString()}\r\nData: {messageEvent.JsonData}");
                currentSubscription?.InvokeExceptionHandler(ex);
                return false;
            }
        }

        /// <summary>
        /// Send data and wait for an answer
        /// </summary>
        /// <typeparam name="T">The data type expected in response</typeparam>
        /// <param name="obj">The object to send</param>
        /// <param name="timeout">The timeout for response</param>
        /// <param name="handler">The response handler</param>
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
            log.Write(LogLevel.Debug, $"Socket {Socket.Id} sending data: {data}");
            Socket.Send(data);
        }

        /// <summary>
        /// Handler for a socket opening
        /// </summary>
        protected virtual void SocketOnOpen()
        {
            ReconnectTry = 0;
            PausedActivity = false;
            Connected = true;
        }

        /// <summary>
        /// Handler for a socket closing. Reconnects the socket if needed, or removes it from the active socket list if not
        /// </summary>
        protected virtual void SocketOnClose()
        {
            lock (pendingRequests)
            {
                foreach(var pendingRequest in pendingRequests.ToList())
                {
                    pendingRequest.Fail();
                    pendingRequests.Remove(pendingRequest);
                }
            }

            if (socketClient.AutoReconnect && ShouldReconnect)
            {
                if (Socket.Reconnecting)
                    return; // Already reconnecting

                Socket.Reconnecting = true;

                DisconnectTime = DateTime.UtcNow;
                log.Write(LogLevel.Information, $"Socket {Socket.Id} Connection lost, will try to reconnect after {socketClient.ReconnectInterval}");
                if (!lostTriggered)
                {
                    lostTriggered = true;
                    ConnectionLost?.Invoke();
                }

                Task.Run(async () =>
                {
                    while (ShouldReconnect)
                    {
                        // Wait a bit before attempting reconnect
                        await Task.Delay(socketClient.ReconnectInterval).ConfigureAwait(false);
                        if (!ShouldReconnect)
                        {
                            // Should reconnect changed to false while waiting to reconnect
                            Socket.Reconnecting = false;
                            return;
                        }

                        Socket.Reset();
                        if (!await Socket.ConnectAsync().ConfigureAwait(false))
                        {
                            ReconnectTry++;
                            ResubscribeTry = 0;
                            if (socketClient.MaxReconnectTries != null
                            && ReconnectTry >= socketClient.MaxReconnectTries)
                            {
                                log.Write(LogLevel.Debug, $"Socket {Socket.Id} failed to reconnect after {ReconnectTry} tries, closing");
                                ShouldReconnect = false;

                                if (socketClient.sockets.ContainsKey(Socket.Id))
                                    socketClient.sockets.TryRemove(Socket.Id, out _);

                                Closed?.Invoke();
                                _ = Task.Run(() => ConnectionClosed?.Invoke());
                                break;
                            }

                            log.Write(LogLevel.Debug, $"Socket {Socket.Id} failed to reconnect{(socketClient.MaxReconnectTries != null ? $", try {ReconnectTry}/{socketClient.MaxReconnectTries}": "")}");
                            continue;
                        }

                        // Successfully reconnected
                        var time = DisconnectTime;
                        DisconnectTime = null;

                        log.Write(LogLevel.Information, $"Socket {Socket.Id} reconnected after {DateTime.UtcNow - time}");

                        var reconnectResult = await ProcessReconnectAsync().ConfigureAwait(false);
                        if (!reconnectResult)
                        {
                            ResubscribeTry++;

                            if (socketClient.MaxResubscribeTries != null &&
                            ResubscribeTry >= socketClient.MaxResubscribeTries)
                            {
                                log.Write(LogLevel.Debug, $"Socket {Socket.Id} failed to resubscribe after {ResubscribeTry} tries, closing");
                                ShouldReconnect = false;

                                if (socketClient.sockets.ContainsKey(Socket.Id))
                                    socketClient.sockets.TryRemove(Socket.Id, out _);

                                Closed?.Invoke();
                                _ = Task.Run(() => ConnectionClosed?.Invoke());
                            }
                            else
                                log.Write(LogLevel.Debug, $"Socket {Socket.Id} resubscribing all subscriptions failed on reconnected socket{(socketClient.MaxResubscribeTries != null ? $", try {ResubscribeTry}/{socketClient.MaxResubscribeTries}" : "")}. Disconnecting and reconnecting.");

                            if (Socket.IsOpen)                            
                                await Socket.CloseAsync().ConfigureAwait(false);                            
                            else
                                DisconnectTime = DateTime.UtcNow;
                        }
                        else
                        {
                            log.Write(LogLevel.Debug, $"Socket {Socket.Id} data connection restored.");
                            ResubscribeTry = 0;
                            if (lostTriggered)
                            {
                                lostTriggered = false;
                                InvokeConnectionRestored(time);
                            }

                            break;
                        }
                    }

                    Socket.Reconnecting = false;
                });
            }
            else
            {
                if (!socketClient.AutoReconnect && ShouldReconnect)
                    _ = Task.Run(() => ConnectionClosed?.Invoke());

                // No reconnecting needed
                log.Write(LogLevel.Information, $"Socket {Socket.Id} closed");
                if (socketClient.sockets.ContainsKey(Socket.Id))
                    socketClient.sockets.TryRemove(Socket.Id, out _);

                Closed?.Invoke();
            }
        }

        private async void InvokeConnectionRestored(DateTime? disconnectTime)
        {
            await Task.Run(() => ConnectionRestored?.Invoke(disconnectTime.HasValue ? DateTime.UtcNow - disconnectTime.Value : TimeSpan.FromSeconds(0))).ConfigureAwait(false);
        }

        private async Task<bool> ProcessReconnectAsync()
        {
            if (Authenticated)
            {
                if (!Socket.IsOpen)
                    return false;

                // If we reconnected a authenticated connection we need to re-authenticate
                var authResult = await socketClient.AuthenticateSocketAsync(this).ConfigureAwait(false);
                if (!authResult)
                {
                    log.Write(LogLevel.Information, $"Socket {Socket.Id} authentication failed on reconnected socket. Disconnecting and reconnecting.");
                    return false;
                }

                log.Write(LogLevel.Debug, $"Socket {Socket.Id} authentication succeeded on reconnected socket.");
            }

            // Get a list of all subscriptions on the socket
            List<SocketSubscription> subscriptionList;
            lock (subscriptionLock)
                subscriptionList = subscriptions.Where(h => h.Request != null).ToList();

            // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
            for (var i = 0; i < subscriptionList.Count; i += socketClient.MaxConcurrentResubscriptionsPerSocket)
            {
                var success = true;
                var taskList = new List<Task>();
                foreach (var subscription in subscriptionList.Skip(i).Take(socketClient.MaxConcurrentResubscriptionsPerSocket))
                {
                    if (!Socket.IsOpen)
                        continue;

                    var task = socketClient.SubscribeAndWaitAsync(this, subscription.Request!, subscription).ContinueWith(t =>
                    {
                        if (!t.Result)
                            success = false;
                    });
                    taskList.Add(task);
                }

                await Task.WhenAll(taskList).ConfigureAwait(false);
                if (!success || !Socket.IsOpen)
                    return false;
            }        

            log.Write(LogLevel.Debug, $"Socket {Socket.Id} all subscription successfully resubscribed on reconnected socket.");
            return true;
        }

        internal async Task UnsubscribeAsync(SocketSubscription socketSubscription)
        {
            await socketClient.UnsubscribeAsync(this, socketSubscription).ConfigureAwait(false);
        }

        internal async Task<CallResult<bool>> ResubscribeAsync(SocketSubscription socketSubscription)
        {
            if (!Socket.IsOpen)
                return new CallResult<bool>(false, new UnknownError("Socket is not connected"));

            return await socketClient.SubscribeAndWaitAsync(this, socketSubscription.Request!, socketSubscription).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            Connected = false;
            ShouldReconnect = false;
            if (socketClient.sockets.ContainsKey(Socket.Id))
                socketClient.sockets.TryRemove(Socket.Id, out _);
            
            await Socket.CloseAsync().ConfigureAwait(false);
            Socket.Dispose();
        }

        /// <summary>
        /// Close a subscription on this connection. If all subscriptions on this connection are closed the connection gets closed as well
        /// </summary>
        /// <param name="subscription">Subscription to close</param>
        /// <returns></returns>
        public async Task CloseAsync(SocketSubscription subscription)
        {
            if (!Socket.IsOpen)
                return;

            if (subscription.Confirmed)
                await socketClient.UnsubscribeAsync(this, subscription).ConfigureAwait(false);

            var shouldCloseConnection = false;
            lock (subscriptionLock)
                shouldCloseConnection = !subscriptions.Any(r => r.UserSubscription && subscription != r);

            if (shouldCloseConnection)
                await CloseAsync().ConfigureAwait(false);

            lock (subscriptionLock)
                subscriptions.Remove(subscription);            
        }
    }

    internal class PendingRequest
    {
        public Func<JToken, bool> Handler { get; }
        public JToken? Result { get; private set; }
        public bool Completed { get; private set; }
        public AsyncResetEvent Event { get; }
        public TimeSpan Timeout { get; }

        private CancellationTokenSource cts;

        public PendingRequest(Func<JToken, bool> handler, TimeSpan timeout)
        {
            Handler = handler;
            Event = new AsyncResetEvent(false, false);
            Timeout = timeout;

            cts = new CancellationTokenSource(timeout);
            cts.Token.Register(Fail, false);
        }

        public bool CheckData(JToken data)
        {
            if (Handler(data))
            {
                Result = data;
                Completed = true;
                Event.Set();
                return true;
            }            

            return false;
        }

        public void Fail()
        {
            Completed = true;
            Event.Set();
        }
    }
}
