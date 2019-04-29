using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    public abstract class SocketClient: BaseClient, ISocketClient
    {
        #region fields
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        public IWebsocketFactory SocketFactory { get; set; } = new WebsocketFactory();

        protected internal List<SocketConnection> sockets = new List<SocketConnection>();
        protected internal readonly object socketLock = new object();

        public TimeSpan ReconnectInterval { get; private set; }
        public bool AutoReconnect { get; private set; }
        public TimeSpan ResponseTimeout { get; private set; }
        public TimeSpan SocketTimeout { get; private set; }
        public int MaxSocketConnections { get; protected set; } = 999;
        public int SocketCombineTarget { get; protected set; } = 1;

        protected Func<byte[], string> dataInterpreterBytes;
        protected Func<string, string> dataInterpreterString;
        protected Dictionary<string, Action<SocketConnection, JToken>> genericHandlers = new Dictionary<string, Action<SocketConnection, JToken>>();
        protected Task periodicTask;
        protected AutoResetEvent periodicEvent;
        protected bool disposing;
        #endregion

        protected SocketClient(SocketClientOptions exchangeOptions, AuthenticationProvider authenticationProvider): base(exchangeOptions, authenticationProvider)
        {
            Configure(exchangeOptions);
        }

        /// <summary>
        /// Configure the client using the provided options
        /// </summary>
        /// <param name="exchangeOptions">Options</param>
        protected void Configure(SocketClientOptions exchangeOptions)
        {
            AutoReconnect = exchangeOptions.AutoReconnect;
            ReconnectInterval = exchangeOptions.ReconnectInterval;
            ResponseTimeout = exchangeOptions.SocketResponseTimeout;
            SocketTimeout = exchangeOptions.SocketNoDataTimeout;
        }

        /// <summary>
        /// Set a function to interpret the data, used when the data is received as bytes instead of a string
        /// </summary>
        /// <param name="handler"></param>
        protected void SetDataInterpreter(Func<byte[], string> byteHandler, Func<string, string> stringHandler)
        {
            dataInterpreterBytes = byteHandler;
            dataInterpreterString = stringHandler;
        }

        protected virtual async Task<CallResult<UpdateSubscription>> Subscribe<T>(object request, string identifier, bool authenticated, Action<T> dataHandler)
        {
            return await Subscribe(BaseAddress, request, identifier, authenticated, dataHandler).ConfigureAwait(false);
        }

        protected virtual async Task<CallResult<UpdateSubscription>> Subscribe<T>(string url, object request, string identifier, bool authenticated, Action<T> dataHandler)
        {
            SocketConnection socket;
            SocketSubscription handler;
            if (SocketCombineTarget == 1)
            {;
                lock (socketLock)
                {
                    socket = GetWebsocket(url, authenticated);
                    handler = AddHandler(request, identifier, true, socket, dataHandler);
                }

                var connectResult = ConnectIfNeeded(socket, authenticated).GetAwaiter().GetResult();
                if (!connectResult.Success)
                    return new CallResult<UpdateSubscription>(null, connectResult.Error);
            }
            else
            {
                lock (socketLock)
                {
                    socket = GetWebsocket(url, authenticated);
                    handler = AddHandler(request, identifier, true, socket, dataHandler);

                    var connectResult = ConnectIfNeeded(socket, authenticated).GetAwaiter().GetResult();
                    if (!connectResult.Success)
                        return new CallResult<UpdateSubscription>(null, connectResult.Error);
                }
            }


            if (request != null)
            {
                var subResult = await SubscribeAndWait(socket, request, handler).ConfigureAwait(false);
                if (!subResult.Success)
                {
                    await socket.Close(handler).ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(null, subResult.Error);
                }

            }
            else
                handler.Confirmed = true;

            socket.ShouldReconnect = true;
            return new CallResult<UpdateSubscription>(new UpdateSubscription(socket, handler), null);
        }

        protected internal virtual async Task<CallResult<bool>> SubscribeAndWait(SocketConnection socket, object request, SocketSubscription subscription)
        {
            CallResult<object> callResult = null;
            await socket.SendAndWait(request, ResponseTimeout, (data) =>
            {
                if (!HandleSubscriptionResponse(socket, subscription, request, data, out callResult))
                    return false;

                return true;
            }).ConfigureAwait(false);

            if (callResult?.Success == true)
                subscription.Confirmed = true;

            return new CallResult<bool>(callResult?.Success ?? false, callResult == null ? new ServerError("No response on subscription request received"): callResult.Error);
        }

        protected virtual async Task<CallResult<T>> Query<T>(object request, bool authenticated)
        {
            var socket = GetWebsocket(BaseAddress, authenticated);
            var connectResult = await ConnectIfNeeded(socket, authenticated).ConfigureAwait(false);
            if (!connectResult.Success) 
                return new CallResult<T>(default(T), connectResult.Error);

            if (socket.PausedActivity)
            {
                log.Write(LogVerbosity.Info, "Socket has been paused, can't send query at this moment");
                return new CallResult<T>(default(T), new ServerError("Socket is paused"));
            }

            return await QueryAndWait<T>(socket, request).ConfigureAwait(false);
        }

        protected virtual async Task<CallResult<T>> QueryAndWait<T>(SocketConnection socket, object request)
        {
            CallResult<T> dataResult = new CallResult<T>(default(T), new ServerError("No response on query received"));
            await socket.SendAndWait(request, ResponseTimeout, (data) =>
            {
                if (!HandleQueryResponse<T>(socket, request, data, out var callResult))
                    return false;

                dataResult = callResult;
                return true;
            }).ConfigureAwait(false);

            return dataResult;
        }

        protected virtual async Task<CallResult<bool>> ConnectIfNeeded(SocketConnection socket, bool authenticated)
        {
            if (!socket.Connected)
            {
                var connectResult = await ConnectSocket(socket).ConfigureAwait(false);
                if (!connectResult.Success)
                {
                    return new CallResult<bool>(false, new CantConnectError());
                }

                if (authenticated && !socket.Authenticated)
                {
                    var result = await AuthenticateSocket(socket).ConfigureAwait(false);
                    if (!result.Success)
                    {
                        log.Write(LogVerbosity.Warning, "Socket authentication failed");
                        result.Error.Message = "Authentication failed: " + result.Error.Message;
                        return new CallResult<bool>(false, result.Error);
                    }

                    socket.Authenticated = true;
                }
            }

            return new CallResult<bool>(true, null);
        }

        protected virtual void AddGenericHandler(string identifier, Action<SocketConnection, JToken> action)
        {
            genericHandlers.Add(identifier, action);
            List<SocketConnection> socketList;
            lock (socketLock)
                socketList = sockets.ToList();
            foreach (var wrapper in socketList)
                wrapper.AddHandler(identifier, false, action);
        }

        protected internal abstract bool HandleQueryResponse<T>(SocketConnection s, object request, JToken data, out CallResult<T> callResult);
        protected internal abstract bool HandleSubscriptionResponse(SocketConnection s, SocketSubscription subscription, object request, JToken message, out CallResult<object> callResult);
        protected internal abstract bool MessageMatchesHandler(JToken message, object request);
        protected internal abstract bool MessageMatchesHandler(JToken message, string identifier);
        protected internal abstract Task<CallResult<bool>> AuthenticateSocket(SocketConnection s);
        protected internal abstract Task<bool> Unsubscribe(SocketConnection connection, SocketSubscription s);
        protected internal virtual JToken ProcessTokenData(JToken message)
        {
            return message;
        }

        protected virtual SocketSubscription AddHandler<T>(object request, string identifier, bool userSubscription, SocketConnection connection, Action<T> dataHandler)
        {
            Action<SocketConnection, JToken> internalHandler = (socketWrapper, data) =>
            {
                if (typeof(T) == typeof(string))
                {
                    dataHandler((T)Convert.ChangeType(data.ToString(), typeof(T)));
                    return;
                }

                var desResult = Deserialize<T>(data, false);
                if (!desResult.Success)
                {
                    log.Write(LogVerbosity.Warning, $"Failed to deserialize data into type {typeof(T)}: {desResult.Error}");
                    return;
                }

                dataHandler(desResult.Data);
            };

            if (request != null)
                return connection.AddHandler(request, userSubscription, internalHandler);
            return connection.AddHandler(identifier, userSubscription, internalHandler);
        }

        protected virtual SocketConnection GetWebsocket(string address, bool authenticated)
        {
            SocketConnection result = sockets.Where(s => s.Socket.Url == address && (s.Authenticated == authenticated || !authenticated) && s.Connected).OrderBy(s => s.HandlerCount).FirstOrDefault();
            if (result != null)
            {
                if (result.HandlerCount < SocketCombineTarget || (sockets.Count >= MaxSocketConnections && sockets.All(s => s.HandlerCount >= SocketCombineTarget)))
                {
                    // Use existing socket if it has less than target connections OR it has the least connections and we can't make new
                    return result;
                }
            }

            // Create new socket
            var socket = CreateSocket(address);
            var socketWrapper = new SocketConnection(this, log, socket);
            foreach (var kvp in genericHandlers)
                socketWrapper.AddHandler(kvp.Key, false, kvp.Value);
            return socketWrapper;
        }

        /// <summary>
        /// Connect a socket
        /// </summary>
        /// <param name="socketConnection">The subscription to connect</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<bool>> ConnectSocket(SocketConnection socketConnection)
        {
            if (await socketConnection.Socket.Connect().ConfigureAwait(false))
            {
                sockets.Add(socketConnection);
                return new CallResult<bool>(true, null);
            }

            socketConnection.Socket.Dispose();
            return new CallResult<bool>(false, new CantConnectError());
        }

        /// <summary>
        /// Create a socket for an address
        /// </summary>
        /// <param name="address">The address the socket should connect to</param>
        /// <returns></returns>
        protected virtual IWebsocket CreateSocket(string address)
        {
            var socket = SocketFactory.CreateWebsocket(log, address);
            log.Write(LogVerbosity.Debug, "Created new socket for " + address);

            if (apiProxy != null)
                socket.SetProxy(apiProxy.Host, apiProxy.Port);

            socket.Timeout = SocketTimeout;
            socket.DataInterpreterBytes = dataInterpreterBytes;
            socket.DataInterpreterString = dataInterpreterString;
            socket.OnError += e =>
            {
                log.Write(LogVerbosity.Info, $"Socket {socket.Id} error: " + e.ToString());
            };
            return socket;
        }

        public virtual void SendPeriodic(TimeSpan interval, Func<SocketConnection, object> objGetter)
        {
            periodicEvent = new AutoResetEvent(false);
            periodicTask = Task.Run(() =>
            {
                while (!disposing)
                {
                    periodicEvent.WaitOne(interval);
                    if (disposing)
                        break;

                    List<SocketConnection> socketList;
                    lock (socketLock)
                        socketList = sockets.ToList();

                    if (socketList.Any())
                        log.Write(LogVerbosity.Debug, "Sending periodic");

                    foreach (var socket in socketList)
                    {
                        if (disposing)
                            break;

                        var obj = objGetter(socket);
                        if (obj != null)
                        {
                            try
                            {
                                socket.Send(obj);
                            }
                            catch (Exception ex)
                            {
                                log.Write(LogVerbosity.Warning, "Periodic send failed: " + ex);
                            }
                        }
                    }
                }

            });
        }
        

        /// <summary>
        /// Unsubscribe from a stream
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task Unsubscribe(UpdateSubscription subscription)
        {
            if (subscription == null)
                return;

            log.Write(LogVerbosity.Info, "Closing subscription");
            await subscription.Close().ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAll()
        {
            lock (socketLock)
                log.Write(LogVerbosity.Debug, $"Closing all {sockets.Count} subscriptions");

            await Task.Run(() =>
            {
                var tasks = new List<Task>();
                lock (socketLock)
                {
                    foreach (var sub in new List<SocketConnection>(sockets))
                        tasks.Add(sub.Close());
                }

                Task.WaitAll(tasks.ToArray());
            }).ConfigureAwait(false);
        }

        public override void Dispose()
        {
            disposing = true;
            periodicEvent?.Set();
            log.Write(LogVerbosity.Debug, "Disposing socket client, closing all subscriptions");
            UnsubscribeAll().Wait();

            base.Dispose();
        }
    }
}
