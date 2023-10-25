using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base socket API client for interaction with a websocket API
    /// </summary>
    public abstract class SocketApiClient : BaseApiClient, ISocketApiClient
    {
        #region Fields
        /// <inheritdoc/>
        public IWebsocketFactory SocketFactory { get; set; } = new WebsocketFactory();

        /// <summary>
        /// List of socket connections currently connecting/connected
        /// </summary>
        protected internal ConcurrentDictionary<int, SocketConnection> socketConnections = new();

        /// <summary>
        /// Semaphore used while creating sockets
        /// </summary>
        protected internal readonly SemaphoreSlim semaphoreSlim = new(1);

        /// <summary>
        /// Keep alive interval for websocket connection
        /// </summary>
        protected TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Delegate used for manipulating data received from socket connections before it is processed by listeners
        /// </summary>
        protected Func<Stream, Stream>? interceptor;

        /// <summary>
        /// Handlers for data from the socket which doesn't need to be forwarded to the caller. Ping or welcome messages for example.
        /// </summary>
        protected List<SystemSubscription> systemSubscriptions = new();

        /// <summary>
        /// The task that is sending periodic data on the websocket. Can be used for sending Ping messages every x seconds or similair. Not necesarry.
        /// </summary>
        protected Task? periodicTask;

        /// <summary>
        /// Wait event for the periodicTask
        /// </summary>
        protected AsyncResetEvent? periodicEvent;

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
        /// The rate limiters 
        /// </summary>
        protected internal IEnumerable<IRateLimiter>? RateLimiters { get; set; }

        /// <inheritdoc />
        public double IncomingKbps
        {
            get
            {
                if (!socketConnections.Any())
                    return 0;

                return socketConnections.Sum(s => s.Value.IncomingKbps);
            }
        }

        /// <inheritdoc />
        public int CurrentConnections => socketConnections.Count;

        /// <inheritdoc />
        public int CurrentSubscriptions
        {
            get
            {
                if (!socketConnections.Any())
                    return 0;

                return socketConnections.Sum(s => s.Value.UserListenerCount);
            }
        }


        /// <inheritdoc />
        public new SocketExchangeOptions ClientOptions => (SocketExchangeOptions)base.ClientOptions;

        /// <inheritdoc />
        public new SocketApiOptions ApiOptions => (SocketApiOptions)base.ApiOptions;
        #endregion

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">log</param>
        /// <param name="options">Client options</param>
        /// <param name="baseAddress">Base address for this API client</param>
        /// <param name="apiOptions">The Api client options</param>
        public SocketApiClient(ILogger logger, string baseAddress, SocketExchangeOptions options, SocketApiOptions apiOptions) 
            : base(logger, 
                  apiOptions.OutputOriginalData ?? options.OutputOriginalData,
                  apiOptions.ApiCredentials ?? options.ApiCredentials,
                  baseAddress,
                  options,
                  apiOptions)
        {
            var rateLimiters = new List<IRateLimiter>();
            foreach (var rateLimiter in apiOptions.RateLimiters)
                rateLimiters.Add(rateLimiter);
            RateLimiters = rateLimiters;
        }

        /// <summary>
        /// Set a delegate which can manipulate the message stream before it is processed by listeners
        /// </summary>
        /// <param name="interceptor">Interceptor</param>
        protected void SetInterceptor(Func<Stream, Stream> interceptor)
        {
            this.interceptor = interceptor;
        }

        /// <summary>
        /// Connect to an url and listen for data on the BaseAddress
        /// </summary>
        /// <typeparam name="T">The type of the expected data</typeparam>
        /// <param name="subscription">The subscription</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        protected virtual Task<CallResult<UpdateSubscription>> SubscribeAsync<T>(Subscription subscription, CancellationToken ct)
        {
            return SubscribeAsync<T>(BaseAddress, subscription, ct);
        }

        /// <summary>
        /// Connect to an url and listen for data
        /// </summary>
        /// <typeparam name="T">The type of the expected data</typeparam>
        /// <param name="url">The URL to connect to</param>
        /// <param name="subscription">The subscription</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<UpdateSubscription>> SubscribeAsync<T>(string url, Subscription subscription, CancellationToken ct)
        {
            if (_disposing)
                return new CallResult<UpdateSubscription>(new InvalidOperationError("Client disposed, can't subscribe"));

            SocketConnection socketConnection;
            MessageListener? messageListener;
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
                while (true)
                {
                    // Get a new or existing socket connection
                    var socketResult = await GetSocketConnection(url, subscription.Authenticated).ConfigureAwait(false);
                    if (!socketResult)
                        return socketResult.As<UpdateSubscription>(null);

                    socketConnection = socketResult.Data;

                    // Add a subscription on the socket connection
                    messageListener = AddSubscription<T>(subscription, true, socketConnection);
                    if (messageListener == null)
                    {
                        _logger.Log(LogLevel.Trace, $"Socket {socketConnection.SocketId} failed to add subscription, retrying on different connection");
                        continue;
                    }

                    if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                    {
                        // Only 1 subscription per connection, so no need to wait for connection since a new subscription will create a new connection anyway
                        semaphoreSlim.Release();
                        released = true;
                    }

                    var needsConnecting = !socketConnection.Connected;

                    var connectResult = await ConnectIfNeededAsync(socketConnection, subscription.Authenticated).ConfigureAwait(false);
                    if (!connectResult)
                        return new CallResult<UpdateSubscription>(connectResult.Error!);

                    break;
                }
            }
            finally
            {
                if (!released)
                    semaphoreSlim.Release();
            }

            if (socketConnection.PausedActivity)
            {
                _logger.Log(LogLevel.Warning, $"Socket {socketConnection.SocketId} has been paused, can't subscribe at this moment");
                return new CallResult<UpdateSubscription>(new ServerError("Socket is paused"));
            }

            var request = subscription.GetSubRequest();
            if (request != null)
            {
                // Send the request and wait for answer
                var subResult = await SubscribeAndWaitAsync(socketConnection, request, messageListener).ConfigureAwait(false);
                if (!subResult)
                {
                    _logger.Log(LogLevel.Warning, $"Socket {socketConnection.SocketId} failed to subscribe: {subResult.Error}");
                    await socketConnection.CloseAsync(messageListener).ConfigureAwait(false);
                    return new CallResult<UpdateSubscription>(subResult.Error!);
                }
            }
            else
            {
                // No request to be sent, so just mark the subscription as comfirmed
                messageListener.Confirmed = true;
            }

            if (ct != default)
            {
                messageListener.CancellationTokenRegistration = ct.Register(async () =>
                {
                    _logger.Log(LogLevel.Information, $"Socket {socketConnection.SocketId} Cancellation token set, closing subscription {messageListener.Id}");
                    await socketConnection.CloseAsync(messageListener).ConfigureAwait(false);
                }, false);
            }

            _logger.Log(LogLevel.Information, $"Socket {socketConnection.SocketId} subscription {messageListener.Id} completed successfully");
            return new CallResult<UpdateSubscription>(new UpdateSubscription(socketConnection, messageListener));
        }

        /// <summary>
        /// Sends the subscribe request and waits for a response to that request
        /// </summary>
        /// <param name="socketConnection">The connection to send the request on</param>
        /// <param name="request">The request to send, will be serialized to json</param>
        /// <param name="listener">The message listener for the subscription</param>
        /// <returns></returns>
        protected internal virtual async Task<CallResult<bool>> SubscribeAndWaitAsync(SocketConnection socketConnection, object request, MessageListener listener)
        {
            CallResult? callResult = null;
            await socketConnection.SendAndWaitAsync(request, ClientOptions.RequestTimeout, listener, 1, x =>
            {
                var (matches, result) = listener.Subscription!.MessageMatchesSubRequest(x);
                if (matches)
                    callResult = result;
                return matches;
            }).ConfigureAwait(false);

            if (callResult?.Success == true)
            {
                listener.Confirmed = true;
                return new CallResult<bool>(true);
            }

            if (callResult == null)
                return new CallResult<bool>(new ServerError("No response on subscription request received"));

            return new CallResult<bool>(callResult.Error!);
        }

        /// <summary>
        /// Send a query on a socket connection to the BaseAddress and wait for the response
        /// </summary>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <param name="query">The query</param>
        /// <returns></returns>
        protected virtual Task<CallResult<T>> QueryAsync<T>(Query query)
        {
            return QueryAsync<T>(BaseAddress, query);
        }

        /// <summary>
        /// Send a query on a socket connection and wait for the response
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="url">The url for the request</param>
        /// <param name="query">The query</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> QueryAsync<T>(string url, Query query)
        {
            if (_disposing)
                return new CallResult<T>(new InvalidOperationError("Client disposed, can't query"));

            SocketConnection socketConnection;
            var released = false;
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                var socketResult = await GetSocketConnection(url, query.Authenticated).ConfigureAwait(false);
                if (!socketResult)
                    return socketResult.As<T>(default);

                socketConnection = socketResult.Data;

                if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                {
                    // Can release early when only a single sub per connection
                    semaphoreSlim.Release();
                    released = true;
                }

                var connectResult = await ConnectIfNeededAsync(socketConnection, query.Authenticated).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult<T>(connectResult.Error!);
            }
            finally
            {
                if (!released)
                    semaphoreSlim.Release();
            }

            if (socketConnection.PausedActivity)
            {
                _logger.Log(LogLevel.Warning, $"Socket {socketConnection.SocketId} has been paused, can't send query at this moment");
                return new CallResult<T>(new ServerError("Socket is paused"));
            }

            return await QueryAndWaitAsync<T>(socketConnection, query).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the query request and waits for the result
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="socket">The connection to send and wait on</param>
        /// <param name="query">The query</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> QueryAndWaitAsync<T>(SocketConnection socket, Query query)
        {
            var dataResult = new CallResult<T>(new ServerError("No response on query received"));
            await socket.SendAndWaitAsync(query.Request, ClientOptions.RequestTimeout, null, query.Weight, x =>
            {
                var matches = query.MessageMatchesQuery(x);
                if (matches)
                {
                    query.HandleResponse(x);
                    return true;
                }

                return false;
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

            if (ClientOptions.DelayAfterConnect != TimeSpan.Zero)
                await Task.Delay(ClientOptions.DelayAfterConnect).ConfigureAwait(false);

            if (!authenticated || socket.Authenticated)
                return new CallResult<bool>(true);

            return await AuthenticateSocketAsync(socket).ConfigureAwait(false);
        }

        /// <summary>
        /// Authenticate a socket connection
        /// </summary>
        /// <param name="socket">Socket to authenticate</param>
        /// <returns></returns>
        public virtual async Task<CallResult<bool>> AuthenticateSocketAsync(SocketConnection socket)
        {
            _logger.Log(LogLevel.Debug, $"Socket {socket.SocketId} Attempting to authenticate");
            var authRequest = GetAuthenticationRequest();
            var authResult = new CallResult(new ServerError("No response from server"));
            await socket.SendAndWaitAsync(authRequest.Request, ClientOptions.RequestTimeout, null, 1, x =>
            {
                var matches = authRequest.MessageMatchesQuery(x);
                if (matches)
                {
                    authResult = authRequest.HandleResponse(x);
                    return true;
                }

                return false;
            }).ConfigureAwait(false);

            if (!authResult)
            {
                _logger.Log(LogLevel.Warning, $"Socket {socket.SocketId} authentication failed");
                if (socket.Connected)
                    await socket.CloseAsync().ConfigureAwait(false);

                authResult.Error!.Message = "Authentication failed: " + authResult.Error.Message;
                return new CallResult<bool>(authResult.Error);
            }

            _logger.Log(LogLevel.Debug, $"Socket {socket.SocketId} authenticated");
            socket.Authenticated = true;
            return new CallResult<bool>(true);
        }

        /// <summary>
        /// Should return the request which can be used to authenticate a socket connection
        /// </summary>
        /// <returns></returns>
        protected internal abstract Query GetAuthenticationRequest();

        /// <summary>
        /// Add a subscription to a connection
        /// </summary>
        /// <typeparam name="T">The type of data the subscription expects</typeparam>
        /// <param name="subscription">The subscription</param>
        /// <param name="userSubscription">Whether or not this is a user subscription (counts towards the max amount of handlers on a socket)</param>
        /// <param name="connection">The socket connection the handler is on</param>
        /// <returns></returns>
        protected virtual MessageListener? AddSubscription<T>(Subscription subscription, bool userSubscription, SocketConnection connection)
        {
            var messageListener = new MessageListener(ExchangeHelpers.NextId(), subscription, userSubscription);
            if (!connection.AddListener(messageListener))
                return null;

            return messageListener;
        }

        /// <summary>
        /// Adds a system subscription. Used for example to reply to ping requests
        /// </summary>
        /// <param name="systemSubscription">The subscription</param>
        protected void AddSystemSubscription(SystemSubscription systemSubscription)
        {
            systemSubscriptions.Add(systemSubscription);
            var subscription = new MessageListener(ExchangeHelpers.NextId(), systemSubscription, false);
            foreach (var connection in socketConnections.Values)
                connection.AddListener(subscription);
        }

        /// <summary>
        /// Get the url to connect to (defaults to BaseAddress form the client options)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="authentication"></param>
        /// <returns></returns>
        protected virtual Task<CallResult<string?>> GetConnectionUrlAsync(string address, bool authentication)
        {
            return Task.FromResult(new CallResult<string?>(address));
        }

        /// <summary>
        /// Get the url to reconnect to after losing a connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        protected internal virtual Task<Uri?> GetReconnectUriAsync(SocketConnection connection)
        {
            return Task.FromResult<Uri?>(connection.ConnectionUri);
        }

        /// <summary>
        /// Update the original request to send when the connection is restored after disconnecting. Can be used to update an authentication token for example.
        /// </summary>
        /// <param name="request">The original request</param>
        /// <returns></returns>
        protected internal virtual Task<CallResult<object>> RevitalizeRequestAsync(object request)
        {
            return Task.FromResult(new CallResult<object>(request));
        }

        /// <summary>
        /// Gets a connection for a new subscription or query. Can be an existing if there are open position or a new one.
        /// </summary>
        /// <param name="address">The address the socket is for</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<SocketConnection>> GetSocketConnection(string address, bool authenticated)
        {
            var socketResult = socketConnections.Where(s => (s.Value.Status == SocketConnection.SocketStatus.None || s.Value.Status == SocketConnection.SocketStatus.Connected)
                                                  && s.Value.Tag.TrimEnd('/') == address.TrimEnd('/')
                                                  && (s.Value.ApiClient.GetType() == GetType())
                                                  && (s.Value.Authenticated == authenticated || !authenticated) && s.Value.Connected).OrderBy(s => s.Value.UserListenerCount).FirstOrDefault();
            var result = socketResult.Equals(default(KeyValuePair<int, SocketConnection>)) ? null : socketResult.Value;
            if (result != null)
            {
                if (result.UserListenerCount < ClientOptions.SocketSubscriptionsCombineTarget || (socketConnections.Count >= (ApiOptions.MaxSocketConnections ?? ClientOptions.MaxSocketConnections) && socketConnections.All(s => s.Value.UserListenerCount >= ClientOptions.SocketSubscriptionsCombineTarget)))
                {
                    // Use existing socket if it has less than target connections OR it has the least connections and we can't make new
                    return new CallResult<SocketConnection>(result);
                }
            }

            var connectionAddress = await GetConnectionUrlAsync(address, authenticated).ConfigureAwait(false);
            if (!connectionAddress)
            {
                _logger.Log(LogLevel.Warning, $"Failed to determine connection url: " + connectionAddress.Error);
                return connectionAddress.As<SocketConnection>(null);
            }

            if (connectionAddress.Data != address)
                _logger.Log(LogLevel.Debug, $"Connection address set to " + connectionAddress.Data);

            // Create new socket
            var socket = CreateSocket(connectionAddress.Data!);
            var socketConnection = new SocketConnection(_logger, this, socket, address);
            socketConnection.UnhandledMessage += HandleUnhandledMessage;

            foreach (var systemSubscription in systemSubscriptions)
            {
                var handler = new MessageListener(ExchangeHelpers.NextId(), systemSubscription, false);
                socketConnection.AddListener(handler);
            }

            return new CallResult<SocketConnection>(socketConnection);
        }

        /// <summary>
        /// Process an unhandled message
        /// </summary>
        /// <param name="message">The message that wasn't processed</param>
        protected virtual void HandleUnhandledMessage(StreamMessage message)
        {
        }

        /// <summary>
        /// Connect a socket
        /// </summary>
        /// <param name="socketConnection">The socket to connect</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<bool>> ConnectSocketAsync(SocketConnection socketConnection)
        {
            if (await socketConnection.ConnectAsync().ConfigureAwait(false))
            {
                socketConnections.TryAdd(socketConnection.SocketId, socketConnection);
                return new CallResult<bool>(true);
            }

            socketConnection.Dispose();
            return new CallResult<bool>(new CantConnectError());
        }

        /// <summary>
        /// Get parameters for the websocket connection
        /// </summary>
        /// <param name="address">The address to connect to</param>
        /// <returns></returns>
        protected virtual WebSocketParameters GetWebSocketParameters(string address)
            => new(new Uri(address), ClientOptions.AutoReconnect)
            {
                Interceptor = interceptor,
                KeepAliveInterval = KeepAliveInterval,
                ReconnectInterval = ClientOptions.ReconnectInterval,
                RateLimiters = RateLimiters,
                Proxy = ClientOptions.Proxy,
                Timeout = ApiOptions.SocketNoDataTimeout ?? ClientOptions.SocketNoDataTimeout
            };

        /// <summary>
        /// Create a socket for an address
        /// </summary>
        /// <param name="address">The address the socket should connect to</param>
        /// <returns></returns>
        protected virtual IWebsocket CreateSocket(string address)
        {
            var socket = SocketFactory.CreateWebsocket(_logger, GetWebSocketParameters(address));
            _logger.Log(LogLevel.Debug, $"Socket {socket.Id} new socket created for " + address);
            return socket;
        }

        /// <summary>
        /// Periodically sends data over a socket connection
        /// </summary>
        /// <param name="identifier">Identifier for the periodic send</param>
        /// <param name="interval">How often</param>
        /// <param name="objGetter">Method returning the object to send</param>
        protected virtual void SendPeriodic(string identifier, TimeSpan interval, Func<SocketConnection, object> objGetter)
        {
            if (objGetter == null)
                throw new ArgumentNullException(nameof(objGetter));

            periodicEvent = new AsyncResetEvent();
            periodicTask = Task.Run(async () =>
            {
                while (!_disposing)
                {
                    await periodicEvent.WaitAsync(interval).ConfigureAwait(false);
                    if (_disposing)
                        break;

                    foreach (var socketConnection in socketConnections.Values)
                    {
                        if (_disposing)
                            break;

                        if (!socketConnection.Connected)
                            continue;

                        var obj = objGetter(socketConnection);
                        if (obj == null)
                            continue;

                        _logger.Log(LogLevel.Trace, $"Socket {socketConnection.SocketId} sending periodic {identifier}");

                        try
                        {
                            socketConnection.Send(ExchangeHelpers.NextId(), obj, 1);
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Warning, $"Socket {socketConnection.SocketId} Periodic send {identifier} failed: " + ex.ToLogString());
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
        public virtual async Task<bool> UnsubscribeAsync(int subscriptionId)
        {
            MessageListener? subscription = null;
            SocketConnection? connection = null;
            foreach (var socket in socketConnections.Values.ToList())
            {
                subscription = socket.GetListener(subscriptionId);
                if (subscription != null)
                {
                    connection = socket;
                    break;
                }
            }

            if (subscription == null || connection == null)
                return false;

            _logger.Log(LogLevel.Information, $"Socket {connection.SocketId} Unsubscribing subscription " + subscriptionId);
            await connection.CloseAsync(subscription).ConfigureAwait(false);
            return true;
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

            _logger.Log(LogLevel.Information, $"Socket {subscription.SocketId} Unsubscribing subscription  " + subscription.Id);
            await subscription.CloseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAllAsync()
        {
            var sum = socketConnections.Sum(s => s.Value.UserListenerCount);
            if (sum == 0)
                return;

            _logger.Log(LogLevel.Information, $"Unsubscribing all {socketConnections.Sum(s => s.Value.UserListenerCount)} subscriptions");
            var tasks = new List<Task>();
            {
                var socketList = socketConnections.Values;
                foreach (var sub in socketList)
                    tasks.Add(sub.CloseAsync()); 
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Reconnect all connections
        /// </summary>
        /// <returns></returns>
        public virtual async Task ReconnectAsync()
        {
            _logger.Log(LogLevel.Information, $"Reconnecting all {socketConnections.Count} connections");
            var tasks = new List<Task>();
            {
                var socketList = socketConnections.Values;
                foreach (var sub in socketList)
                    tasks.Add(sub.TriggerReconnectAsync());
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Log the current state of connections and subscriptions
        /// </summary>
        public string GetSubscriptionsState()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{socketConnections.Count} connections, {CurrentSubscriptions} subscriptions, kbps: {IncomingKbps}");
            foreach (var connection in socketConnections)
            {
                sb.AppendLine($"  Connection {connection.Key}: {connection.Value.UserListenerCount} subscriptions, status: {connection.Value.Status}, authenticated: {connection.Value.Authenticated}, kbps: {connection.Value.IncomingKbps}");
                foreach (var subscription in connection.Value.MessageListeners)
                    sb.AppendLine($"    Subscription {subscription.Id}, authenticated: {subscription.Authenticated}, confirmed: {subscription.Confirmed}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Dispose the client
        /// </summary>
        public override void Dispose()
        {
            _disposing = true;
            periodicEvent?.Set();
            periodicEvent?.Dispose();
            if (socketConnections.Sum(s => s.Value.UserListenerCount) > 0)
            {
                _logger.Log(LogLevel.Debug, "Disposing socket client, closing all subscriptions");
                _ = UnsubscribeAllAsync();
            }
            semaphoreSlim?.Dispose();
            base.Dispose();
        }
    }
}
