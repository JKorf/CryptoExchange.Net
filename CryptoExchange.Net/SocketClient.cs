using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// <summary>
    /// Base for socket client implementations
    /// </summary>
    public abstract class SocketClient: BaseClient, ISocketClient
    {
        #region fields
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        public IWebsocketFactory SocketFactory { get; set; } = new WebsocketFactory();

        /// <summary>
        /// List of socket connections currently connecting/connected
        /// </summary>
        protected internal ConcurrentDictionary<int, SocketConnection> sockets = new ConcurrentDictionary<int, SocketConnection>();
        /// <summary>
        /// </summary>
        protected internal readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        /// <inheritdoc cref="SocketClientOptions.ReconnectInterval"/>
        public TimeSpan ReconnectInterval { get; }
        /// <inheritdoc cref="SocketClientOptions.AutoReconnect"/>
        public bool AutoReconnect { get; }
        /// <inheritdoc cref="SocketClientOptions.SocketResponseTimeout"/>
        public TimeSpan ResponseTimeout { get; }
        /// <inheritdoc cref="SocketClientOptions.SocketNoDataTimeout"/>
        public TimeSpan SocketNoDataTimeout { get; }
        /// <summary>
        /// The max amount of concurrent socket connections
        /// </summary>
        public int MaxSocketConnections { get; protected set; } = 9999;
        /// <inheritdoc cref="SocketClientOptions.SocketSubscriptionsCombineTarget"/>
        public int SocketCombineTarget { get; protected set; }

        /// <summary>
        /// Handler for byte data
        /// </summary>
        protected Func<byte[], string>? dataInterpreterBytes;
        /// <summary>
        /// Handler for string data
        /// </summary>
        protected Func<string, string>? dataInterpreterString;
        /// <summary>
        /// Generic handlers
        /// </summary>
        protected Dictionary<string, Action<SocketConnection, JToken>> genericHandlers = new Dictionary<string, Action<SocketConnection, JToken>>();
        /// <summary>
        /// Periodic task
        /// </summary>
        protected Task? periodicTask;
        /// <summary>
        /// Periodic task event
        /// </summary>
        protected AutoResetEvent? periodicEvent;
        /// <summary>
        /// Is disposing
        /// </summary>
        protected bool disposing;

        /// <summary>
        /// If true; data which is a response to a query will also be distributed to subscriptions
        /// If false; data which is a response to a query won't get forwarded to subscriptions as well
        /// </summary>
        protected internal bool ContinueOnQueryResponse { get; protected set; }
        #endregion

        /// <summary>
        /// Create a socket client
        /// </summary>
        /// <param name="exchangeOptions">Client options</param>
        /// <param name="authenticationProvider">Authentication provider</param>
        protected SocketClient(SocketClientOptions exchangeOptions, AuthenticationProvider? authenticationProvider): base(exchangeOptions, authenticationProvider)
        {
            if (exchangeOptions == null)
                throw new ArgumentNullException(nameof(exchangeOptions));

            AutoReconnect = exchangeOptions.AutoReconnect;
            ReconnectInterval = exchangeOptions.ReconnectInterval;
            ResponseTimeout = exchangeOptions.SocketResponseTimeout;
            SocketNoDataTimeout = exchangeOptions.SocketNoDataTimeout;
            SocketCombineTarget = exchangeOptions.SocketSubscriptionsCombineTarget ?? 1;
        }

        /// <summary>
        /// Set a function to interpret the data, used when the data is received as bytes instead of a string
        /// </summary>
        /// <param name="byteHandler">Handler for byte data</param>
        /// <param name="stringHandler">Handler for string data</param>
        protected void SetDataInterpreter(Func<byte[], string>? byteHandler, Func<string, string>? stringHandler)
        {
            dataInterpreterBytes = byteHandler;
            dataInterpreterString = stringHandler;
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <typeparam name="T">The expected return data</typeparam>
        /// <param name="request">The request to send</param>
        /// <param name="identifier">The identifier to use</param>
        /// <param name="authenticated">If the subscription should be authenticated</param>
        /// <param name="dataHandler">The handler of update data</param>
        /// <returns></returns>
        protected virtual Task<CallResult<UpdateSubscription>> Subscribe<T>(object? request, string? identifier, bool authenticated, Action<T> dataHandler)
        {
            return Subscribe(BaseAddress, request, identifier, authenticated, dataHandler);
        }

        /// <summary>
        /// Subscribe using a specif URL
        /// </summary>
        /// <typeparam name="T">The type of the expected data</typeparam>
        /// <param name="url">The URL to connect to</param>
        /// <param name="request">The request to send</param>
        /// <param name="identifier">The identifier to use</param>
        /// <param name="authenticated">If the subscription should be authenticated</param>
        /// <param name="dataHandler">The handler of update data</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<UpdateSubscription>> Subscribe<T>(string url, object? request, string? identifier, bool authenticated, Action<T> dataHandler)
        {
            SocketConnection socket;
            SocketSubscription handler;
            var released = false;
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                socket = GetWebsocket(url, authenticated);
                handler = AddHandler(request, identifier, true, socket, dataHandler);
                if (SocketCombineTarget == 1)
                {
                    // Can release early when only a single sub per connection
                    semaphoreSlim.Release();
                    released = true;
                }

                var connectResult = await ConnectIfNeeded(socket, authenticated).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult<UpdateSubscription>(null, connectResult.Error);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                if(!released)
                    semaphoreSlim.Release();
            }

            if (socket.PausedActivity)
            {
                log.Write(LogVerbosity.Info, "Socket has been paused, can't subscribe at this moment");
                return new CallResult<UpdateSubscription>(default, new ServerError("Socket is paused"));
            }

            if (request != null)
            {
                var subResult = await SubscribeAndWait(socket, request, handler).ConfigureAwait(false);
                if (!subResult)
                {
                    await socket.Close(handler).ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(null, subResult.Error);
                }
            }
            else
            {
                handler.Confirmed = true;
            }
            
            socket.ShouldReconnect = true;
            return new CallResult<UpdateSubscription>(new UpdateSubscription(socket, handler), null);
        }

        /// <summary>
        /// Sends the subscribe request and waits for a response to that request
        /// </summary>
        /// <param name="socket">The connection to send the request on</param>
        /// <param name="request">The request to send</param>
        /// <param name="subscription">The subscription the request is for</param>
        /// <returns></returns>
        protected internal virtual async Task<CallResult<bool>> SubscribeAndWait(SocketConnection socket, object request, SocketSubscription subscription)
        {
            CallResult<object>? callResult = null;
            await socket.SendAndWait(request, ResponseTimeout, data => HandleSubscriptionResponse(socket, subscription, request, data, out callResult)).ConfigureAwait(false);

            if (callResult?.Success == true)
                subscription.Confirmed = true;

            return new CallResult<bool>(callResult?.Success ?? false, callResult == null ? new ServerError("No response on subscription request received"): callResult.Error);
        }

        /// <summary>
        /// Query for data
        /// </summary>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <param name="request">The request to send</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <returns></returns>
        protected virtual Task<CallResult<T>> Query<T>(object request, bool authenticated)
        {
            return Query<T>(BaseAddress, request, authenticated);
        }

        /// <summary>
        /// Query for data
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="url">The url for the request</param>
        /// <param name="request">The request to send</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> Query<T>(string url, object request, bool authenticated)
        {
            SocketConnection socket;
            var released = false;
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                socket = GetWebsocket(url, authenticated);
                if (SocketCombineTarget == 1)
                {
                    // Can release early when only a single sub per connection
                    semaphoreSlim.Release();
                    released = true;
                }

                var connectResult = await ConnectIfNeeded(socket, authenticated).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult<T>(default, connectResult.Error);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                if (!released)
                    semaphoreSlim.Release();
            }

            if (socket.PausedActivity)
            {
                log.Write(LogVerbosity.Info, "Socket has been paused, can't send query at this moment");
                return new CallResult<T>(default, new ServerError("Socket is paused"));
            }

            return await QueryAndWait<T>(socket, request).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the query request and waits for the result
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="socket">The connection to send and wait on</param>
        /// <param name="request">The request to send</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> QueryAndWait<T>(SocketConnection socket, object request)
        {
            var dataResult = new CallResult<T>(default, new ServerError("No response on query received"));
            await socket.SendAndWait(request, ResponseTimeout, data =>
            {
                if (!HandleQueryResponse<T>(socket, request, data, out var callResult))
                    return false;

                dataResult = callResult;
                return true;
            }).ConfigureAwait(false);

            return dataResult;
        }

        /// <summary>
        /// Checks if a socket needs to be connected and does so if needed
        /// </summary>
        /// <param name="socket">The connection to check</param>
        /// <param name="authenticated">Whether the socket should authenticated</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<bool>> ConnectIfNeeded(SocketConnection socket, bool authenticated)
        {
            if (socket.Connected)
                return new CallResult<bool>(true, null);

            var connectResult = await ConnectSocket(socket).ConfigureAwait(false);
            if (!connectResult)
                return new CallResult<bool>(false, new CantConnectError());

            if (!authenticated || socket.Authenticated)
                return new CallResult<bool>(true, null);

            var result = await AuthenticateSocket(socket).ConfigureAwait(false);
            if (!result)
            {
                log.Write(LogVerbosity.Warning, "Socket authentication failed");
                result.Error!.Message = "Authentication failed: " + result.Error.Message;
                return new CallResult<bool>(false, result.Error);
            }

            socket.Authenticated = true;
            return new CallResult<bool>(true, null);
        }
        
        /// <summary>
        /// Needs to check if a received message was an answer to a query request (preferable by id) and set the callResult out to whatever the response is
        /// </summary>
        /// <typeparam name="T">The type of response</typeparam>
        /// <param name="s">The socket connection</param>
        /// <param name="request">The request that a response is awaited for</param>
        /// <param name="data">The message</param>
        /// <param name="callResult">The interpretation (null if message wasn't a response to the request)</param>
        /// <returns>True if the message was a response to the query</returns>
        protected internal abstract bool HandleQueryResponse<T>(SocketConnection s, object request, JToken data, [NotNullWhen(true)]out CallResult<T>? callResult);
        /// <summary>
        /// Needs to check if a received message was an answer to a subscription request (preferable by id) and set the callResult out to whatever the response is
        /// </summary>
        /// <param name="s">The socket connection</param>
        /// <param name="subscription"></param>
        /// <param name="request">The request that a response is awaited for</param>
        /// <param name="message">The message</param>
        /// <param name="callResult">The interpretation (null if message wasn't a response to the request)</param>
        /// <returns>True if the message was a response to the subscription request</returns>
        protected internal abstract bool HandleSubscriptionResponse(SocketConnection s, SocketSubscription subscription, object request, JToken message, out CallResult<object>? callResult);
        /// <summary>
        /// Needs to check if a received message matches a handler. Typically if an update message matches the request
        /// </summary>
        /// <param name="message">The received data</param>
        /// <param name="request">The subscription request</param>
        /// <returns></returns>
        protected internal abstract bool MessageMatchesHandler(JToken message, object request);
        /// <summary>
        /// Needs to check if a received message matches a handler. Typically if an received message matches a ping request or a other information pushed from the the server
        /// </summary>
        /// <param name="message">The received data</param>
        /// <param name="identifier">The string identifier of the handler</param>
        /// <returns></returns>
        protected internal abstract bool MessageMatchesHandler(JToken message, string identifier);
        /// <summary>
        /// Needs to authenticate the socket so authenticated queries/subscriptions can be made on this socket connection
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected internal abstract Task<CallResult<bool>> AuthenticateSocket(SocketConnection s);
        /// <summary>
        /// Needs to unsubscribe a subscription, typically by sending an unsubscribe request. If multiple subscriptions per socket is not allowed this can just return since the socket will be closed anyway
        /// </summary>
        /// <param name="connection">The connection on which to unsubscribe</param>
        /// <param name="s">The subscription to unsubscribe</param>
        /// <returns></returns>
        protected internal abstract Task<bool> Unsubscribe(SocketConnection connection, SocketSubscription s);

        /// <summary>
        /// Optional handler to interpolate data before sending it to the handlers
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected internal virtual JToken ProcessTokenData(JToken message)
        {
            return message;
        }

        /// <summary>
        /// Add a handler for a subscription
        /// </summary>
        /// <typeparam name="T">The type of data the subscription expects</typeparam>
        /// <param name="request">The request of the subscription</param>
        /// <param name="identifier">The identifier of the subscription (can be null if request param is used)</param>
        /// <param name="userSubscription">Whether or not this is a user subscription (counts towards the max amount of handlers on a socket)</param>
        /// <param name="connection">The socket connection the handler is on</param>
        /// <param name="dataHandler">The handler of the data received</param>
        /// <returns></returns>
        protected virtual SocketSubscription AddHandler<T>(object? request, string? identifier, bool userSubscription, SocketConnection connection, Action<T> dataHandler)
        {
            void InternalHandler(SocketConnection socketWrapper, JToken data)
            {
                if (typeof(T) == typeof(string))
                {
                    dataHandler((T) Convert.ChangeType(data.ToString(), typeof(T)));
                    return;
                }

                var desResult = Deserialize<T>(data, false);
                if (!desResult)
                {
                    log.Write(LogVerbosity.Warning, $"Failed to deserialize data into type {typeof(T)}: {desResult.Error}");
                    return;
                }

                dataHandler(desResult.Data);
            }

            var handler = request == null
                ? SocketSubscription.CreateForIdentifier(identifier!, userSubscription, InternalHandler)
                : SocketSubscription.CreateForRequest(request, userSubscription, InternalHandler);
            connection.AddHandler(handler);
            return handler;
        }

        /// <summary>
        /// Adds a generic message handler. Used for example to reply to ping requests
        /// </summary>
        /// <param name="identifier">The name of the request handler. Needs to be unique</param>
        /// <param name="action">The action to execute when receiving a message for this handler (checked by <see cref="MessageMatchesHandler(Newtonsoft.Json.Linq.JToken,string)"/>)</param>
        protected void AddGenericHandler(string identifier, Action<SocketConnection, JToken> action)
        {
            genericHandlers.Add(identifier, action);
            var handler = SocketSubscription.CreateForIdentifier(identifier, false, action);
            foreach (var connection in sockets.Values)
                connection.AddHandler(handler);
        }

        /// <summary>
        /// Gets a connection for a new subscription or query. Can be an existing if there are open position or a new one.
        /// </summary>
        /// <param name="address">The address the socket is for</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <returns></returns>
        protected virtual SocketConnection GetWebsocket(string address, bool authenticated)
        {
            var socketResult = sockets.Where(s => s.Value.Socket.Url == address && (s.Value.Authenticated == authenticated || !authenticated) && s.Value.Connected).OrderBy(s => s.Value.HandlerCount).FirstOrDefault();
            var result = socketResult.Equals(default(KeyValuePair<int, SocketConnection>)) ? null : socketResult.Value;
            if (result != null)
            {
                if (result.HandlerCount < SocketCombineTarget || (sockets.Count >= MaxSocketConnections && sockets.All(s => s.Value.HandlerCount >= SocketCombineTarget)))
                {
                    // Use existing socket if it has less than target connections OR it has the least connections and we can't make new
                    return result;
                }
            }

            // Create new socket
            var socket = CreateSocket(address);
            var socketWrapper = new SocketConnection(this, socket);
            foreach (var kvp in genericHandlers)
            {
                var handler = SocketSubscription.CreateForIdentifier(kvp.Key, false, kvp.Value);
                socketWrapper.AddHandler(handler);
            }

            return socketWrapper;
        }

        /// <summary>
        /// Connect a socket
        /// </summary>
        /// <param name="socketConnection">The socket to connect</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<bool>> ConnectSocket(SocketConnection socketConnection)
        {
            if (await socketConnection.Socket.Connect().ConfigureAwait(false))
            {
                sockets.TryAdd(socketConnection.Socket.Id, socketConnection);
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

            socket.Timeout = SocketNoDataTimeout;
            socket.DataInterpreterBytes = dataInterpreterBytes;
            socket.DataInterpreterString = dataInterpreterString;
            socket.OnError += e =>
            {
                log.Write(LogVerbosity.Info, $"Socket {socket.Id} error: " + e);
            };
            return socket;
        }

        /// <summary>
        /// Periodically sends an object to a socket
        /// </summary>
        /// <param name="interval">How often</param>
        /// <param name="objGetter">Method returning the object to send</param>
        public virtual void SendPeriodic(TimeSpan interval, Func<SocketConnection, object> objGetter)
        {
            if (objGetter == null)
                throw new ArgumentNullException(nameof(objGetter));

            periodicEvent = new AutoResetEvent(false);
            periodicTask = Task.Run(async () =>
            {
                while (!disposing)
                {
                    await periodicEvent.WaitOneAsync(interval).ConfigureAwait(false);
                    if (disposing)
                        break;
                    
                    if (sockets.Any())
                        log.Write(LogVerbosity.Debug, "Sending periodic");

                    foreach (var socket in sockets.Values)
                    {
                        if (disposing)
                            break;

                        var obj = objGetter(socket);
                        if (obj == null)
                            continue;

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
                throw new ArgumentNullException(nameof(subscription));

            log.Write(LogVerbosity.Info, "Closing subscription");
            await subscription.Close().ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAll()
        {
            log.Write(LogVerbosity.Debug, $"Closing all {sockets.Sum(s => s.Value.HandlerCount)} subscriptions");

            await Task.Run(async () =>
            {
                var tasks = new List<Task>();
                {
                    var socketList = sockets.Values;
                    foreach (var sub in socketList)
                        tasks.Add(sub.Close());
                }

                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose the client
        /// </summary>
        public override void Dispose()
        {
            disposing = true;
            periodicEvent?.Set();
            periodicEvent?.Dispose();
            log.Write(LogVerbosity.Debug, "Disposing socket client, closing all subscriptions");
            UnsubscribeAll().Wait();
            semaphoreSlim?.Dispose();
            base.Dispose();
        }
    }
}
