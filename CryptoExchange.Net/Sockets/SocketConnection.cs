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
    public class SocketConnection
    {
        public event Action ConnectionLost;
        public event Action<TimeSpan> ConnectionRestored;
        public event Action Closed;

        public int HandlerCount
        {
            get { lock (handlersLock)
                return handlers.Count(h => h.UserSubscription); }
        }

        public bool Authenticated { get; set; }
        public bool Connected { get; private set; }


        public IWebsocket Socket { get; set; }
        public bool ShouldReconnect { get; set; }
        public DateTime? DisconnectTime { get; set; }
        public bool PausedActivity { get; set; }

        internal readonly List<SocketSubscription> handlers;
        private readonly object handlersLock = new object();

        private bool lostTriggered;
        private readonly Log log;
        private readonly SocketClient socketClient;

        private readonly List<PendingRequest> pendingRequests;

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

        public SocketSubscription AddHandler(object request, bool userSubscription, Action<SocketConnection, JToken> dataHandler)
        {
            var handler = new SocketSubscription(null, request, userSubscription, dataHandler);
            lock (handlersLock)
                handlers.Add(handler);
            return handler;
        }

        public SocketSubscription AddHandler(string identifier, bool userSubscription, Action<SocketConnection, JToken> dataHandler)
        {
            var handler = new SocketSubscription(identifier, null, userSubscription, dataHandler);
            lock (handlersLock)
                handlers.Add(handler);
            return handler;
        }

        public void ProcessMessage(string data)
        {
            log.Write(LogVerbosity.Debug, $"Socket {Socket.Id} received data: " + data);
            var tokenData = data.ToJToken(log);
            if (tokenData == null)
                return;

            foreach (var pendingRequest in pendingRequests.ToList())
            {
                if (pendingRequest.Check(tokenData))
                {
                    pendingRequests.Remove(pendingRequest);
                    return;
                }
            }

            if (!HandleData(tokenData))
            {
                log.Write(LogVerbosity.Debug, "Message not handled: " + tokenData);
            }
        }

        private bool HandleData(JToken tokenData)
        {
            SocketSubscription currentSubscription = null;
            try
            { 
                bool handled = false;
                var sw = Stopwatch.StartNew();
                lock (handlersLock)
                {
                    foreach (var handler in handlers)
                    {
                        currentSubscription = handler;
                        if (handler.Request == null)
                        {
                            if (socketClient.MessageMatchesHandler(tokenData, handler.Identifier))
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
                                Task.Run(() => ConnectionRestored?.Invoke(DisconnectTime.HasValue ? DateTime.UtcNow - DisconnectTime.Value : TimeSpan.FromSeconds(0)));
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
                Socket.Dispose();
                Closed?.Invoke();
            }
        }

        public async Task<bool> ProcessReconnect()
        {
            if (Authenticated)
            {
                var authResult = await socketClient.AuthenticateSocket(this).ConfigureAwait(false);
                if (!authResult.Success)
                {
                    log.Write(LogVerbosity.Info, "Authentication failed on reconnected socket. Disconnecting and reconnecting.");
                    return false;
                }

                log.Write(LogVerbosity.Debug, "Authentication succeeded on reconnected socket.");
            }

            List<SocketSubscription> handlerList;
            lock (handlersLock)
                handlerList = handlers.Where(h => h.Request != null).ToList();
            foreach (var handler in handlerList)
            {
                var resubResult = await socketClient.SubscribeAndWait(this, handler.Request, handler).ConfigureAwait(false);
                if (!resubResult.Success)
                {
                    log.Write(LogVerbosity.Debug, "Resubscribing all subscriptions failed on reconnected socket. Disconnecting and reconnecting.");
                    return false;
                }
            }

            log.Write(LogVerbosity.Debug, "All subscription successfully resubscribed on reconnected socket.");
            return true;
        }
        
        public async Task Close()
        {
            Connected = false;
            ShouldReconnect = false;
            if (socketClient.sockets.ContainsKey(Socket.Id))
                socketClient.sockets.TryRemove(Socket.Id, out _);
            
            await Socket.Close().ConfigureAwait(false);
            Socket.Dispose();
        }

        public async Task Close(SocketSubscription subscription)
        {
            if (subscription.Confirmed)
                await socketClient.Unsubscribe(this, subscription).ConfigureAwait(false);

            bool shouldCloseWrapper = false;
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

    public class PendingRequest
    {
        public Func<JToken, bool> Handler { get; }
        public JToken Result { get; private set; }
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
