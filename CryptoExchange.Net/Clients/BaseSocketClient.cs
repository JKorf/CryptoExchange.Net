using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base for socket client implementations
    /// </summary>
    public abstract class BaseSocketClient: BaseClient, ISocketClient
    {
        #region fields
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        public IWebsocketFactory SocketFactory { get; set; } = new WebsocketFactory();

        /// <summary>
        /// List of socket connections currently connecting/connected
        /// </summary>
        protected internal ConcurrentDictionary<int, SocketConnection> sockets = new();
        /// <summary>
        /// Semaphore used while creating sockets
        /// </summary>
        protected internal readonly SemaphoreSlim semaphoreSlim = new(1);
        /// <summary>
        /// The max amount of concurrent socket connections
        /// </summary>
        protected int MaxSocketConnections { get; set; } = 9999;        
        /// <summary>
        /// Delegate used for processing byte data received from socket connections before it is processed by handlers
        /// </summary>
        protected Func<byte[], string>? dataInterpreterBytes;
        /// <summary>
        /// Delegate used for processing string data received from socket connections before it is processed by handlers
        /// </summary>
        protected Func<string, string>? dataInterpreterString;
        /// <summary>
        /// Handlers for data from the socket which doesn't need to be forwarded to the caller. Ping or welcome messages for example.
        /// </summary>
        protected Dictionary<string, Action<MessageEvent>> genericHandlers = new();
        /// <summary>
        /// The task that is sending periodic data on the websocket. Can be used for sending Ping messages every x seconds or similair. Not necesarry.
        /// </summary>
        protected Task? periodicTask;
        /// <summary>
        /// Wait event for the periodicTask
        /// </summary>
        protected AsyncResetEvent? periodicEvent;
        /// <summary>
        /// If client is disposing
        /// </summary>
        protected bool disposing;

        /// <summary>
        /// If true; data which is a response to a query will also be distributed to subscriptions
        /// If false; data which is a response to a query won't get forwarded to subscriptions as well
        /// </summary>
        protected internal bool ContinueOnQueryResponse { get; protected set; }

        /// <summary>
        /// If a message is received on the socket which is not handled by a handler this boolean determines whether this logs an error message
        /// </summary>
        protected internal bool UnhandledMessageExpected { get; set; }

        /// <summary>
        /// The max amount of outgoing messages per socket per second
        /// </summary>
        protected internal int? RateLimitPerSocketPerSecond { get; set; }

        /// <inheritdoc />
        public double IncomingKbps
        {
            get
            {
                if (!sockets.Any())
                    return 0;

                return sockets.Sum(s => s.Value.Socket.IncomingKbps);
            }
        }

        /// <summary>
        /// Client options
        /// </summary>
        public new BaseSocketClientOptions ClientOptions { get; }

        #endregion

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">The name of the API this client is for</param>
        /// <param name="options">The options for this client</param>
        protected BaseSocketClient(string name, BaseSocketClientOptions options) : base(name, options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            ClientOptions = options;
        }

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {
            foreach (var apiClient in ApiClients)
                apiClient.SetApiCredentials(credentials);
        }

        /// <summary>
        /// Set a delegate to be used for processing data received from socket connections before it is processed by handlers
        /// </summary>
        /// <param name="byteHandler">Handler for byte data</param>
        /// <param name="stringHandler">Handler for string data</param>
        protected void SetDataInterpreter(Func<byte[], string>? byteHandler, Func<string, string>? stringHandler)
        {
            dataInterpreterBytes = byteHandler;
            dataInterpreterString = stringHandler;
        }

        /// <summary>
        /// Connect to an url and listen for data on the BaseAddress
        /// </summary>
        /// <typeparam name="T">The type of the expected data</typeparam>
        /// <param name="apiClient">The API client the subscription is for</param>
        /// <param name="request">The optional request object to send, will be serialized to json</param>
        /// <param name="identifier">The identifier to use, necessary if no request object is sent</param>
        /// <param name="authenticated">If the subscription is to an authenticated endpoint</param>
        /// <param name="dataHandler">The handler of update data</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        protected virtual Task<CallResult<UpdateSubscription>> SubscribeAsync<T>(SocketApiClient apiClient, object? request, string? identifier, bool authenticated, Action<DataEvent<T>> dataHandler, CancellationToken ct)
        {
            return SubscribeAsync(apiClient, apiClient.Options.BaseAddress, request, identifier, authenticated, dataHandler, ct);
        }

        /// <summary>
        /// Connect to an url and listen for data
        /// </summary>
        /// <typeparam name="T">The type of the expected data</typeparam>
        /// <param name="apiClient">The API client the subscription is for</param>
        /// <param name="url">The URL to connect to</param>
        /// <param name="request">The optional request object to send, will be serialized to json</param>
        /// <param name="identifier">The identifier to use, necessary if no request object is sent</param>
        /// <param name="authenticated">If the subscription is to an authenticated endpoint</param>
        /// <param name="dataHandler">The handler of update data</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<UpdateSubscription>> SubscribeAsync<T>(SocketApiClient apiClient, string url, object? request, string? identifier, bool authenticated, Action<DataEvent<T>> dataHandler, CancellationToken ct)
        {
            if (disposing)
                return new CallResult<UpdateSubscription>(new InvalidOperationError("Client disposed, can't subscribe"));

            SocketConnection socketConnection;
            SocketSubscription subscription;
            var released = false;
            // Wait for a semaphore here, so we only connect 1 socket at a time.
            // This is necessary for being able to see if connections can be combined
            try
            {
                await semaphoreSlim.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return new CallResult<UpdateSubscription>(new CancellationRequestedError());
            }

            try
            {
                // Get a new or existing socket connection
                socketConnection = GetSocketConnection(apiClient, url, authenticated);

                // Add a subscription on the socket connection
                subscription = AddSubscription(request, identifier, true, socketConnection, dataHandler);
                if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                {
                    // Only 1 subscription per connection, so no need to wait for connection since a new subscription will create a new connection anyway
                    semaphoreSlim.Release();
                    released = true;
                }

                var needsConnecting = !socketConnection.Connected;

                var connectResult = await ConnectIfNeededAsync(socketConnection, authenticated).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult<UpdateSubscription>(connectResult.Error!);
            }
            finally
            {
                if(!released)
                    semaphoreSlim.Release();
            }

            if (socketConnection.PausedActivity)
            {
                log.Write(LogLevel.Warning, $"Socket {socketConnection.Socket.Id} has been paused, can't subscribe at this moment");
                return new CallResult<UpdateSubscription>( new ServerError("Socket is paused"));
            }

            if (request != null)
            {
                // Send the request and wait for answer
                var subResult = await SubscribeAndWaitAsync(socketConnection, request, subscription).ConfigureAwait(false);
                if (!subResult)
                {
                    await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(subResult.Error!);
                }
            }
            else
            {
                // No request to be sent, so just mark the subscription as comfirmed
                subscription.Confirmed = true;
            }

            socketConnection.ShouldReconnect = true;
            if (ct != default)
            {
                subscription.CancellationTokenRegistration = ct.Register(async () =>
                {
                    log.Write(LogLevel.Information, $"Socket {socketConnection.Socket.Id} Cancellation token set, closing subscription");
                    await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                }, false);
            }

            log.Write(LogLevel.Information, $"Socket {socketConnection.Socket.Id} subscription completed");
            return new CallResult<UpdateSubscription>(new UpdateSubscription(socketConnection, subscription));
        }

        /// <summary>
        /// Sends the subscribe request and waits for a response to that request
        /// </summary>
        /// <param name="socketConnection">The connection to send the request on</param>
        /// <param name="request">The request to send, will be serialized to json</param>
        /// <param name="subscription">The subscription the request is for</param>
        /// <returns></returns>
        protected internal virtual async Task<CallResult<bool>> SubscribeAndWaitAsync(SocketConnection socketConnection, object request, SocketSubscription subscription)
        {
            CallResult<object>? callResult = null;
            await socketConnection.SendAndWaitAsync(request, ClientOptions.SocketResponseTimeout, data => HandleSubscriptionResponse(socketConnection, subscription, request, data, out callResult)).ConfigureAwait(false);

            if (callResult?.Success == true)
            {
                subscription.Confirmed = true;
                return new CallResult<bool>(true);
            }

            if(callResult== null)
                return new CallResult<bool>(new ServerError("No response on subscription request received"));

            return new CallResult<bool>(callResult.Error!);
        }

        /// <summary>
        /// Send a query on a socket connection to the BaseAddress and wait for the response
        /// </summary>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <param name="apiClient">The API client the query is for</param>
        /// <param name="request">The request to send, will be serialized to json</param>
        /// <param name="authenticated">If the query is to an authenticated endpoint</param>
        /// <returns></returns>
        protected virtual Task<CallResult<T>> QueryAsync<T>(SocketApiClient apiClient, object request, bool authenticated)
        {
            return QueryAsync<T>(apiClient, apiClient.Options.BaseAddress, request, authenticated);
        }

        /// <summary>
        /// Send a query on a socket connection and wait for the response
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="apiClient">The API client the query is for</param>
        /// <param name="url">The url for the request</param>
        /// <param name="request">The request to send</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> QueryAsync<T>(SocketApiClient apiClient, string url, object request, bool authenticated)
        {
            if (disposing)
                return new CallResult<T>(new InvalidOperationError("Client disposed, can't query"));

            SocketConnection socketConnection;
            var released = false;
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                socketConnection = GetSocketConnection(apiClient, url, authenticated);
                if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                {
                    // Can release early when only a single sub per connection
                    semaphoreSlim.Release();
                    released = true;
                }

                var connectResult = await ConnectIfNeededAsync(socketConnection, authenticated).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult<T>(connectResult.Error!);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                if (!released)
                    semaphoreSlim.Release();
            }

            if (socketConnection.PausedActivity)
            {
                log.Write(LogLevel.Warning, $"Socket {socketConnection.Socket.Id} has been paused, can't send query at this moment");
                return new CallResult<T>(new ServerError("Socket is paused"));
            }

            return await QueryAndWaitAsync<T>(socketConnection, request).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the query request and waits for the result
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="socket">The connection to send and wait on</param>
        /// <param name="request">The request to send</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> QueryAndWaitAsync<T>(SocketConnection socket, object request)
        {
            var dataResult = new CallResult<T>(new ServerError("No response on query received"));
            await socket.SendAndWaitAsync(request, ClientOptions.SocketResponseTimeout, data =>
            {
                if (!HandleQueryResponse<T>(socket, request, data, out var callResult))
                    return false;

                dataResult = callResult;
                return true;
            }).ConfigureAwait(false);

            return dataResult;
        }

        /// <summary>
        /// Checks if a socket needs to be connected and does so if needed. Also authenticates on the socket if needed
        /// </summary>
        /// <param name="socket">The connection to check</param>
        /// <param name="authenticated">Whether the socket should authenticated</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<bool>> ConnectIfNeededAsync(SocketConnection socket, bool authenticated)
        {
            if (socket.Connected)
                return new CallResult<bool>(true);

            var connectResult = await ConnectSocketAsync(socket).ConfigureAwait(false);
            if (!connectResult)
                return new CallResult<bool>(connectResult.Error!);

            if (!authenticated || socket.Authenticated)
                return new CallResult<bool>(true);

            var result = await AuthenticateSocketAsync(socket).ConfigureAwait(false);
            if (!result)
            {
                await socket.CloseAsync().ConfigureAwait(false);
                log.Write(LogLevel.Warning, $"Socket {socket.Socket.Id} authentication failed");
                result.Error!.Message = "Authentication failed: " + result.Error.Message;
                return new CallResult<bool>(result.Error);
            }

            socket.Authenticated = true;
            return new CallResult<bool>(true);
        }

        /// <summary>
        /// The socketConnection received data (the data JToken parameter). The implementation of this method should check if the received data is a response to the query that was send (the request parameter).
        /// For example; A query is sent in a request message with an Id parameter with value 10. The socket receives data and calls this method to see if the data it received is an
        /// anwser to any query that was done. The implementation of this method should check if the response.Id == request.Id to see if they match (assuming the api has some sort of Id tracking on messages,
        /// if not some other method has be implemented to match the messages).
        /// If the messages match, the callResult out parameter should be set with the deserialized data in the from of (T) and return true.
        /// </summary>
        /// <typeparam name="T">The type of response that is expected on the query</typeparam>
        /// <param name="socketConnection">The socket connection</param>
        /// <param name="request">The request that a response is awaited for</param>
        /// <param name="data">The message received from the server</param>
        /// <param name="callResult">The interpretation (null if message wasn't a response to the request)</param>
        /// <returns>True if the message was a response to the query</returns>
        protected internal abstract bool HandleQueryResponse<T>(SocketConnection socketConnection, object request, JToken data, [NotNullWhen(true)]out CallResult<T>? callResult);
        /// <summary>
        /// The socketConnection received data (the data JToken parameter). The implementation of this method should check if the received data is a response to the subscription request that was send (the request parameter).
        /// For example; A subscribe request message is send with an Id parameter with value 10. The socket receives data and calls this method to see if the data it received is an
        /// anwser to any subscription request that was done. The implementation of this method should check if the response.Id == request.Id to see if they match (assuming the api has some sort of Id tracking on messages,
        /// if not some other method has be implemented to match the messages).
        /// If the messages match, the callResult out parameter should be set with the deserialized data in the from of (T) and return true.
        /// </summary>
        /// <param name="socketConnection">The socket connection</param>
        /// <param name="subscription">A subscription that waiting for a subscription response</param>
        /// <param name="request">The request that the subscription sent</param>
        /// <param name="data">The message received from the server</param>
        /// <param name="callResult">The interpretation (null if message wasn't a response to the request)</param>
        /// <returns>True if the message was a response to the subscription request</returns>
        protected internal abstract bool HandleSubscriptionResponse(SocketConnection socketConnection, SocketSubscription subscription, object request, JToken data, out CallResult<object>? callResult);
        /// <summary>
        /// Needs to check if a received message matches a handler by request. After subscribing data message will come in. These data messages need to be matched to a specific connection
        /// to pass the correct data to the correct handler. The implementation of this method should check if the message received matches the subscribe request that was sent.
        /// </summary>
        /// <param name="socketConnection">The socket connection the message was recieved on</param>
        /// <param name="message">The received data</param>
        /// <param name="request">The subscription request</param>
        /// <returns>True if the message is for the subscription which sent the request</returns>
        protected internal abstract bool MessageMatchesHandler(SocketConnection socketConnection, JToken message, object request);
        /// <summary>
        /// Needs to check if a received message matches a handler by identifier. Generally used by GenericHandlers. For example; a generic handler is registered which handles ping messages
        /// from the server. This method should check if the message received is a ping message and the identifer is the identifier of the GenericHandler
        /// </summary>
        /// <param name="socketConnection">The socket connection the message was recieved on</param>
        /// <param name="message">The received data</param>
        /// <param name="identifier">The string identifier of the handler</param>
        /// <returns>True if the message is for the handler which has the identifier</returns>
        protected internal abstract bool MessageMatchesHandler(SocketConnection socketConnection, JToken message, string identifier);
        /// <summary>
        /// Needs to authenticate the socket so authenticated queries/subscriptions can be made on this socket connection
        /// </summary>
        /// <param name="socketConnection">The socket connection that should be authenticated</param>
        /// <returns></returns>
        protected internal abstract Task<CallResult<bool>> AuthenticateSocketAsync(SocketConnection socketConnection);
        /// <summary>
        /// Needs to unsubscribe a subscription, typically by sending an unsubscribe request. If multiple subscriptions per socket is not allowed this can just return since the socket will be closed anyway
        /// </summary>
        /// <param name="connection">The connection on which to unsubscribe</param>
        /// <param name="subscriptionToUnsub">The subscription to unsubscribe</param>
        /// <returns></returns>
        protected internal abstract Task<bool> UnsubscribeAsync(SocketConnection connection, SocketSubscription subscriptionToUnsub);

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
        /// Add a subscription to a connection
        /// </summary>
        /// <typeparam name="T">The type of data the subscription expects</typeparam>
        /// <param name="request">The request of the subscription</param>
        /// <param name="identifier">The identifier of the subscription (can be null if request param is used)</param>
        /// <param name="userSubscription">Whether or not this is a user subscription (counts towards the max amount of handlers on a socket)</param>
        /// <param name="connection">The socket connection the handler is on</param>
        /// <param name="dataHandler">The handler of the data received</param>
        /// <returns></returns>
        protected virtual SocketSubscription AddSubscription<T>(object? request, string? identifier, bool userSubscription, SocketConnection connection, Action<DataEvent<T>> dataHandler)
        {
            void InternalHandler(MessageEvent messageEvent)
            {
                if (typeof(T) == typeof(string))
                {
                    var stringData = (T)Convert.ChangeType(messageEvent.JsonData.ToString(), typeof(T));
                    dataHandler(new DataEvent<T>(stringData, null, ClientOptions.OutputOriginalData ? messageEvent.OriginalData : null, messageEvent.ReceivedTimestamp));
                    return;
                }

                var desResult = Deserialize<T>(messageEvent.JsonData);
                if (!desResult)
                {
                    log.Write(LogLevel.Warning, $"Socket {connection.Socket.Id} Failed to deserialize data into type {typeof(T)}: {desResult.Error}");
                    return;
                }

                dataHandler(new DataEvent<T>(desResult.Data, null, ClientOptions.OutputOriginalData ? messageEvent.OriginalData : null, messageEvent.ReceivedTimestamp));
            }

            var subscription = request == null
                ? SocketSubscription.CreateForIdentifier(NextId(), identifier!, userSubscription, InternalHandler)
                : SocketSubscription.CreateForRequest(NextId(), request, userSubscription, InternalHandler);
            connection.AddSubscription(subscription);
            return subscription;
        }

        /// <summary>
        /// Adds a generic message handler. Used for example to reply to ping requests
        /// </summary>
        /// <param name="identifier">The name of the request handler. Needs to be unique</param>
        /// <param name="action">The action to execute when receiving a message for this handler (checked by <see cref="MessageMatchesHandler(SocketConnection, Newtonsoft.Json.Linq.JToken,string)"/>)</param>
        protected void AddGenericHandler(string identifier, Action<MessageEvent> action)
        {
            genericHandlers.Add(identifier, action);
            var subscription = SocketSubscription.CreateForIdentifier(NextId(), identifier, false, action);
            foreach (var connection in sockets.Values)
                connection.AddSubscription(subscription);
        }

        /// <summary>
        /// Gets a connection for a new subscription or query. Can be an existing if there are open position or a new one.
        /// </summary>
        /// <param name="apiClient">The API client the connection is for</param>
        /// <param name="address">The address the socket is for</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <returns></returns>
        protected virtual SocketConnection GetSocketConnection(SocketApiClient apiClient, string address, bool authenticated)
        {
            var socketResult = sockets.Where(s => s.Value.Socket.Url.TrimEnd('/') == address.TrimEnd('/')
                                                  && (s.Value.ApiClient.GetType() == apiClient.GetType())
                                                  && (s.Value.Authenticated == authenticated || !authenticated) && s.Value.Connected).OrderBy(s => s.Value.SubscriptionCount).FirstOrDefault();
            var result = socketResult.Equals(default(KeyValuePair<int, SocketConnection>)) ? null : socketResult.Value;
            if (result != null)
            {
                if (result.SubscriptionCount < ClientOptions.SocketSubscriptionsCombineTarget || (sockets.Count >= MaxSocketConnections && sockets.All(s => s.Value.SubscriptionCount >= ClientOptions.SocketSubscriptionsCombineTarget)))
                {
                    // Use existing socket if it has less than target connections OR it has the least connections and we can't make new
                    return result;
                }
            }

            // Create new socket
            var socket = CreateSocket(address);
            var socketConnection = new SocketConnection(this, apiClient, socket);
            socketConnection.UnhandledMessage += HandleUnhandledMessage;
            foreach (var kvp in genericHandlers)
            {
                var handler = SocketSubscription.CreateForIdentifier(NextId(), kvp.Key, false, kvp.Value);
                socketConnection.AddSubscription(handler);
            }

            return socketConnection;
        }

        /// <summary>
        /// Process an unhandled message
        /// </summary>
        /// <param name="token">The token that wasn't processed</param>
        protected virtual void HandleUnhandledMessage(JToken token)
        {
        }

        /// <summary>
        /// Connect a socket
        /// </summary>
        /// <param name="socketConnection">The socket to connect</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<bool>> ConnectSocketAsync(SocketConnection socketConnection)
        {
            if (await socketConnection.Socket.ConnectAsync().ConfigureAwait(false))
            {
                sockets.TryAdd(socketConnection.Socket.Id, socketConnection);
                return new CallResult<bool>(true);
            }

            socketConnection.Socket.Dispose();
            return new CallResult<bool>(new CantConnectError());
        }

        /// <summary>
        /// Create a socket for an address
        /// </summary>
        /// <param name="address">The address the socket should connect to</param>
        /// <returns></returns>
        protected virtual IWebsocket CreateSocket(string address)
        {
            var socket = SocketFactory.CreateWebsocket(log, address);
            log.Write(LogLevel.Debug, $"Socket {socket.Id} new socket created for " + address);

            if (ClientOptions.Proxy != null)
                socket.SetProxy(ClientOptions.Proxy);

            socket.Timeout = ClientOptions.SocketNoDataTimeout;
            socket.DataInterpreterBytes = dataInterpreterBytes;
            socket.DataInterpreterString = dataInterpreterString;
            socket.RatelimitPerSecond = RateLimitPerSocketPerSecond;
            socket.OnError += e =>
            {
                if(e is WebSocketException wse)
                    log.Write(LogLevel.Warning, $"Socket {socket.Id} error: Websocket error code {wse.WebSocketErrorCode}, details: " + e.ToLogString());
                else
                    log.Write(LogLevel.Warning, $"Socket {socket.Id} error: " + e.ToLogString());
            };
            return socket;
        }

        /// <summary>
        /// Periodically sends data over a socket connection
        /// </summary>
        /// <param name="identifier">Identifier for the periodic send</param>
        /// <param name="interval">How often</param>
        /// <param name="objGetter">Method returning the object to send</param>
        public virtual void SendPeriodic(string identifier, TimeSpan interval, Func<SocketConnection, object> objGetter)
        {
            if (objGetter == null)
                throw new ArgumentNullException(nameof(objGetter));

            periodicEvent = new AsyncResetEvent();
            periodicTask = Task.Run(async () =>
            {
                while (!disposing)
                {
                    await periodicEvent.WaitAsync(interval).ConfigureAwait(false);
                    if (disposing)
                        break;
                    
                    foreach (var socket in sockets.Values)
                    {
                        if (disposing)
                            break;

                        if (!socket.Socket.IsOpen)
                            continue;

                        var obj = objGetter(socket);
                        if (obj == null)
                            continue;

                        log.Write(LogLevel.Trace, $"Socket {socket.Socket.Id} sending periodic {identifier}");

                        try
                        {
                            socket.Send(obj);
                        }
                        catch (Exception ex)
                        {
                            log.Write(LogLevel.Warning, $"Socket {socket.Socket.Id} Periodic send {identifier} failed: " + ex.ToLogString());
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task UnsubscribeAsync(int subscriptionId)
        {

            SocketSubscription? subscription = null;
            SocketConnection? connection = null;
            foreach(var socket in sockets.Values.ToList())
            {
                subscription = socket.GetSubscription(subscriptionId);
                if (subscription != null)
                {
                    connection = socket;
                    break;
                }
            }

            if (subscription == null || connection == null)
                return;

            log.Write(LogLevel.Information, "Closing subscription " + subscriptionId);
            await connection.CloseAsync(subscription).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task UnsubscribeAsync(UpdateSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            log.Write(LogLevel.Information, "Closing subscription " + subscription.Id);
            await subscription.CloseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAllAsync()
        {
            log.Write(LogLevel.Information, $"Closing all {sockets.Sum(s => s.Value.SubscriptionCount)} subscriptions");

            await Task.Run(async () =>
            {
                var tasks = new List<Task>();
                {
                    var socketList = sockets.Values;
                    foreach (var sub in socketList)
                        tasks.Add(sub.CloseAsync());
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
            log.Write(LogLevel.Debug, "Disposing socket client, closing all subscriptions");
            Task.Run(UnsubscribeAllAsync).ConfigureAwait(false).GetAwaiter().GetResult();
            semaphoreSlim?.Dispose();
            base.Dispose();
        }
    }
}
