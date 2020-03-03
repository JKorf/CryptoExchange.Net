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
        /// The amount of handlers
        /// </summary>
        public int HandlerCount
        {
            get { lock (handlersLock)
                return handlers.Count(h => h.UserSubscription); }
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
        /// The socket
        /// </summary>
        public IWebsocket Socket { get; set; }
        /// <summary>
        /// If should reconnect upon closing
        /// </summary>
        public bool ShouldReconnect { get; set; }

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
                    log.Write(LogVerbosity.Debug, "Paused activity: " + value);
                    if(pausedActivity) ActivityPaused?.Invoke();
                    else ActivityUnpaused?.Invoke();
                }
            }
        }

        private bool pausedActivity;
        private readonly List<SocketSubscription> handlers;
        private readonly object handlersLock = new object();

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

            handlers = new List<SocketSubscription>();
            Socket = socket;

            Socket.Timeout = client.SocketNoDataTimeout;
            Socket.OnMessage += ProcessMessage;
            Socket.OnClose += () =>
            {
                if (lostTriggered)
                    return;

                DisconnectTime = DateTime.UtcNow;
                lostTriggered = true;
                
                if (ShouldReconnect)
                    ConnectionLost?.Invoke();
            };
            Socket.OnClose += SocketOnClose;
            Socket.OnOpen += () =>
            {
                PausedActivity = false;
                Connected = true;
            };
        }
        
        private void ProcessMessage(string data)
        {
            log.Write(LogVerbosity.Debug, $"Socket {Socket.Id} received data: " + data);
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
            foreach (var pendingRequest in pendingRequests.ToList())
            {
                if (pendingRequest.Check(tokenData))
                {
                    pendingRequests.Remove(pendingRequest);
                    if (!socketClient.ContinueOnQueryResponse)
                        return;
                    handledResponse = true;
                    break;
                }
            }
            
            if (!HandleData(tokenData) && !handledResponse)
            {
                log.Write(LogVerbosity.Debug, "Message not handled: " + tokenData);
            }
        }

        /// <summary>
        /// Add handler
        /// </summary>
        /// <param name="handler"></param>
        public void AddHandler(SocketSubscription handler)
        {
            lock(handlersLock)
                handlers.Add(handler);
        }

        private bool HandleData(JToken tokenData)
        {
            SocketSubscription? currentSubscription = null;
            try
            { 
                var handled = false;
                var sw = Stopwatch.StartNew();
                lock (handlersLock)
                {
                    foreach (var handler in handlers.ToList())
                    {
                        currentSubscription = handler;
                        if (handler.Request == null)
                        {
                            if (socketClient.MessageMatchesHandler(tokenData, handler.Identifier!))
                            {
                                handled = true;
                                handler.MessageHandler(this, tokenData);
                            }
                        }
                        else
                        {
                            if (socketClient.MessageMatchesHandler(tokenData, handler.Request))
                            {
                                handled = true;
                                tokenData = socketClient.ProcessTokenData(tokenData);
                                handler.MessageHandler(this, tokenData);
                            }
                        }
                    }
                }

                sw.Stop();
                if (sw.ElapsedMilliseconds > 500)
                    log.Write(LogVerbosity.Warning, $"Socket {Socket.Id} message processing slow ({sw.ElapsedMilliseconds}ms), consider offloading data handling to another thread. " +
                                                    "Data from this socket may arrive late or not at all if message processing is continuously slow.");
                return handled;
            }
            catch (Exception ex)
            {
                log.Write(LogVerbosity.Error, $"Socket {Socket.Id} Exception during message processing\r\nException: {ex}\r\nData: {tokenData}");
                currentSubscription?.InvokeExceptionHandler(ex);
                return false;
            }
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="obj">The object to send</param>
        /// <param name="timeout">The timeout for response</param>
        /// <param name="handler">The response handler</param>
        /// <returns></returns>
        public virtual Task SendAndWait<T>(T obj, TimeSpan timeout, Func<JToken, bool> handler)
        {
            var pending = new PendingRequest(handler, timeout);
            pendingRequests.Add(pending);
            Send(obj);
            return pending.Event.WaitOneAsync(timeout);
        }

        /// <summary>
        /// Send data to the websocket
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
        /// Send string data to the websocket
        /// </summary>
        /// <param name="data">The data to send</param>
        public virtual void Send(string data)
        {
            log.Write(LogVerbosity.Debug, $"Socket {Socket.Id} sending data: {data}");
            Socket.Send(data);
        }

        /// <summary>
        /// Handler for a socket closing. Reconnects the socket if needed, or removes it from the active socket list if not
        /// </summary>
        protected virtual void SocketOnClose()
        {
            if (socketClient.AutoReconnect && ShouldReconnect)
            {
                if (Socket.Reconnecting)
                    return; // Already reconnecting

                Socket.Reconnecting = true;

                log.Write(LogVerbosity.Info, $"Socket {Socket.Id} Connection lost, will try to reconnect after {socketClient.ReconnectInterval}");
                Task.Run(async () =>
                {
                    while (ShouldReconnect)
                    {
                        await Task.Delay(socketClient.ReconnectInterval).ConfigureAwait(false);
                        if (!ShouldReconnect)
                        {
                            // Should reconnect changed to false while waiting to reconnect
                            Socket.Reconnecting = false;
                            return;
                        }

                        Socket.Reset();
                        if (!await Socket.Connect().ConfigureAwait(false))
                        {
                            log.Write(LogVerbosity.Debug, $"Socket {Socket.Id} failed to reconnect");
                            continue;
                        }

                        var time = DisconnectTime;
                        DisconnectTime = null;

                        log.Write(LogVerbosity.Info, $"Socket {Socket.Id} reconnected after {DateTime.UtcNow - time}");

                        var reconnectResult = await ProcessReconnect().ConfigureAwait(false);
                        if (!reconnectResult)
                            await Socket.Close().ConfigureAwait(false);
                        else
                        {
                            if (lostTriggered)
                            {
                                lostTriggered = false;
                                InvokeConnectionRestored();
                            }

                            break;
                        }
                    }

                    Socket.Reconnecting = false;
                });
            }
            else
            {
                log.Write(LogVerbosity.Info, $"Socket {Socket.Id} closed");
                if (socketClient.sockets.ContainsKey(Socket.Id))
                    socketClient.sockets.TryRemove(Socket.Id, out _);

                Socket.Dispose();
                Closed?.Invoke();
            }
        }

        private async void InvokeConnectionRestored()
        {
            await Task.Run(() => ConnectionRestored?.Invoke(DisconnectTime.HasValue ? DateTime.UtcNow - DisconnectTime.Value : TimeSpan.FromSeconds(0))).ConfigureAwait(false);
        }

        private async Task<bool> ProcessReconnect()
        {
            if (Authenticated)
            {
                var authResult = await socketClient.AuthenticateSocket(this).ConfigureAwait(false);
                if (!authResult)
                {
                    log.Write(LogVerbosity.Info, "Authentication failed on reconnected socket. Disconnecting and reconnecting.");
                    return false;
                }

                log.Write(LogVerbosity.Debug, "Authentication succeeded on reconnected socket.");
            }

            List<SocketSubscription> handlerList;
            lock (handlersLock)
                handlerList = handlers.Where(h => h.Request != null).ToList();

            var success = true;
            var taskList = new List<Task>();
            foreach (var handler in handlerList)
            {
                var task = socketClient.SubscribeAndWait(this, handler.Request!, handler).ContinueWith(t =>
                {
                    if (!t.Result)
                        success = false;
                });
                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());
            if (!success)
            {
                log.Write(LogVerbosity.Debug, "Resubscribing all subscriptions failed on reconnected socket. Disconnecting and reconnecting.");
                return false;
            }

            log.Write(LogVerbosity.Debug, "All subscription successfully resubscribed on reconnected socket.");
            return true;
        }
        
        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        public async Task Close()
        {
            Connected = false;
            ShouldReconnect = false;
            if (socketClient.sockets.ContainsKey(Socket.Id))
                socketClient.sockets.TryRemove(Socket.Id, out _);
            
            await Socket.Close().ConfigureAwait(false);
            Socket.Dispose();
        }

        /// <summary>
        /// Close the subscription
        /// </summary>
        /// <param name="subscription">Subscription to close</param>
        /// <returns></returns>
        public async Task Close(SocketSubscription subscription)
        {
            if (subscription.Confirmed)
                await socketClient.Unsubscribe(this, subscription).ConfigureAwait(false);

            var shouldCloseWrapper = false;
            lock (handlersLock)
            {
                handlers.Remove(subscription);
                if (handlers.Count(r => r.UserSubscription) == 0)
                    shouldCloseWrapper = true;
            }

            if (shouldCloseWrapper)
                await Close().ConfigureAwait(false);
        }
    }

    internal class PendingRequest
    {
        public Func<JToken, bool> Handler { get; }
        public JToken? Result { get; private set; }
        public ManualResetEvent Event { get; }
        public TimeSpan Timeout { get; }

        private readonly DateTime startTime;

        public PendingRequest(Func<JToken, bool> handler, TimeSpan timeout)
        {
            Handler = handler;
            Event = new ManualResetEvent(false);
            Timeout = timeout;
            startTime = DateTime.UtcNow;
        }

        public bool Check(JToken data)
        {
            if (Handler(data))
            {
                Result = data;
                Event.Set();
                return true;
            }

            if (DateTime.UtcNow - startTime > Timeout)
            {
                // Timed out
                Event.Set();
                return true;
            }

            return false;
        }
    }
}
