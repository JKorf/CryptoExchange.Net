using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.HighPerf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Clients
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
        /// List of HighPerf socket connections currently connecting/connected
        /// </summary>
        protected internal ConcurrentDictionary<int, HighPerfSocketConnection> highPerfSocketConnections = new();

        /// <summary>
        /// Semaphore used while creating sockets
        /// </summary>
        protected internal readonly SemaphoreSlim semaphoreSlim = new(1);

        /// <summary>
        /// Keep alive interval for websocket connection
        /// </summary>
        protected TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Keep alive timeout for websocket connection
        /// </summary>
        protected TimeSpan KeepAliveTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Handlers for data from the socket which doesn't need to be forwarded to the caller. Ping or welcome messages for example.
        /// </summary>
        protected List<SystemSubscription> systemSubscriptions = new();

        /// <summary>
        /// If a message is received on the socket which is not handled by a handler this boolean determines whether this logs an error message
        /// </summary>
        protected internal bool UnhandledMessageExpected { get; set; }

        /// <summary>
        /// The rate limiters 
        /// </summary>
        protected internal IRateLimitGate? RateLimiter { get; set; }

        /// <summary>
        /// The max size a websocket message size can be
        /// </summary>
        protected internal int? MessageSendSizeLimit { get; set; }

        /// <summary>
        /// Periodic task registrations
        /// </summary>
        protected List<PeriodicTaskRegistration> PeriodicTaskRegistrations { get; set; } = new List<PeriodicTaskRegistration>();
        
        /// <summary>
        /// List of address to keep an alive connection to
        /// </summary>
        protected List<DedicatedConnectionConfig> DedicatedConnectionConfigs { get; set; } = new List<DedicatedConnectionConfig>();

        /// <summary>
        /// Whether to allow multiple subscriptions with the same topic on the same connection
        /// </summary>
        protected bool AllowTopicsOnTheSameConnection { get; set; } = true;

        /// <summary>
        /// Whether to continue processing and forward unparsable messages to handlers
        /// </summary>
        protected internal bool ProcessUnparsableMessages { get; set; } = false;

        /// <inheritdoc />
        public double IncomingKbps
        {
            get
            {
                if (socketConnections.IsEmpty)
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
                if (socketConnections.IsEmpty)
                    return 0;

                return socketConnections.Sum(s => s.Value.UserSubscriptionCount);
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
        }

        /// <summary>
        /// Create a message accessor instance
        /// </summary>
        /// <returns></returns>
        protected internal abstract IByteMessageAccessor CreateAccessor(WebSocketMessageType messageType);

        /// <summary>
        /// Create a serializer instance
        /// </summary>
        /// <returns></returns>
        protected internal abstract IMessageSerializer CreateSerializer();

        /// <summary>
        /// Keep an open connection to this url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="auth"></param>
        protected virtual void SetDedicatedConnection(string url, bool auth)
        {
            DedicatedConnectionConfigs.Add(new DedicatedConnectionConfig() { SocketAddress = url, Authenticated = auth });
        }

        /// <summary>
        /// Add a query to periodically send on each connection
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="interval"></param>
        /// <param name="queryDelegate"></param>
        /// <param name="callback"></param>
        protected virtual void RegisterPeriodicQuery(string identifier, TimeSpan interval, Func<ISocketConnection, Query> queryDelegate, Action<SocketConnection, CallResult>? callback)
        {
            PeriodicTaskRegistrations.Add(new PeriodicTaskRegistration
            {
                Identifier = identifier,
                Callback = callback,
                Interval = interval,
                QueryDelegate = queryDelegate
            });
        }

        /// <summary>
        /// Connect to an url and listen for data on the BaseAddress
        /// </summary>
        /// <param name="subscription">The subscription</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        protected virtual Task<CallResult<UpdateSubscription>> SubscribeAsync(Subscription subscription, CancellationToken ct)
        {
            return SubscribeAsync(BaseAddress, subscription, ct);
        }

        /// <summary>
        /// Connect to an url and listen for data
        /// </summary>
        /// <param name="url">The URL to connect to</param>
        /// <param name="subscription">The subscription</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<UpdateSubscription>> SubscribeAsync(string url, Subscription subscription, CancellationToken ct)
        {
            if (_disposing)
                return new CallResult<UpdateSubscription>(new InvalidOperationError("Client disposed, can't subscribe"));

            if (subscription.Authenticated && AuthenticationProvider == null)
            {
                _logger.LogWarning("Failed to subscribe, private subscription but no API credentials set");
                return new CallResult<UpdateSubscription>(new NoApiCredentialsError());
            }

            SocketConnection socketConnection;
            var released = false;
            // Wait for a semaphore here, so we only connect 1 socket at a time.
            // This is necessary for being able to see if connections can be combined
            try
            {
                await semaphoreSlim.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException tce)
            {
                return new CallResult<UpdateSubscription>(new CancellationRequestedError(tce));
            }

            try
            {
                while (true)
                {
                    // Get a new or existing socket connection
                    var socketResult = await GetSocketConnection(url, subscription.Authenticated, false, ct, subscription.Topic).ConfigureAwait(false);
                    if (!socketResult)
                        return socketResult.As<UpdateSubscription>(null);

                    socketConnection = socketResult.Data;

                    // Add a subscription on the socket connection
                    var success = socketConnection.AddSubscription(subscription);
                    if (!success)
                    {
                        _logger.FailedToAddSubscriptionRetryOnDifferentConnection(socketConnection.SocketId);
                        continue;
                    }

                    if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                    {
                        // Only 1 subscription per connection, so no need to wait for connection since a new subscription will create a new connection anyway
                        semaphoreSlim.Release();
                        released = true;
                    }

                    var needsConnecting = !socketConnection.Connected;

                    var connectResult = await ConnectIfNeededAsync(socketConnection, subscription.Authenticated, ct).ConfigureAwait(false);
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
                _logger.HasBeenPausedCantSubscribeAtThisMoment(socketConnection.SocketId);
                return new CallResult<UpdateSubscription>(new ServerError(new ErrorInfo(ErrorType.WebsocketPaused, "Socket is paused")));
            }

            void HandleSubscriptionComplete(bool success, object? response)
            {
                if (!success)
                    return;

                subscription.HandleSubQueryResponse(response);
                subscription.Status = SubscriptionStatus.Subscribed;
                if (ct != default)
                {
                    subscription.CancellationTokenRegistration = ct.Register(async () =>
                    {
                        _logger.CancellationTokenSetClosingSubscription(socketConnection.SocketId, subscription.Id);
                        await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                    }, false);
                }
            }

            subscription.Status = SubscriptionStatus.Subscribing;
            var subQuery = subscription.CreateSubscriptionQuery(socketConnection);
            if (subQuery != null)
            {
                subQuery.OnComplete = () => HandleSubscriptionComplete(subQuery.Result?.Success ?? false, subQuery.Response);

                // Send the request and wait for answer
                var subResult = await socketConnection.SendAndWaitQueryAsync(subQuery, ct).ConfigureAwait(false);
                if (!subResult)
                {
                    var isTimeout = subResult.Error is CancellationRequestedError;
                    if (isTimeout && subscription.Status == SubscriptionStatus.Subscribed)
                    {
                        // No response received, but the subscription did receive updates. We'll assume success
                    }
                    else
                    {
                        _logger.FailedToSubscribe(socketConnection.SocketId, subResult.Error?.ToString());
                        // If this was a server process error we still might need to send an unsubscribe to prevent messages coming in later
                        subscription.Status = SubscriptionStatus.Pending;
                        await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                        return new CallResult<UpdateSubscription>(subResult.Error!);
                    }
                }
            }
            else 
            {
                HandleSubscriptionComplete(true, null);
            }

            _logger.SubscriptionCompletedSuccessfully(socketConnection.SocketId, subscription.Id);
            return new CallResult<UpdateSubscription>(new UpdateSubscription(socketConnection, subscription));
        }

        /// <summary>
        /// Connect to an url and listen for data
        /// </summary>
        /// <param name="url">The URL to connect to</param>
        /// <param name="subscription">The subscription</param>
        /// <param name="connectionFactory">The factory for creating a socket connection</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<HighPerfUpdateSubscription>> SubscribeHighPerfAsync<TUpdateType>(
            string url,
            HighPerfSubscription<TUpdateType> subscription,
            IHighPerfConnectionFactory connectionFactory,
            CancellationToken ct)
        {
            if (_disposing)
                return new CallResult<HighPerfUpdateSubscription>(new InvalidOperationError("Client disposed, can't subscribe"));

            HighPerfSocketConnection<TUpdateType> socketConnection;
            var released = false;
            // Wait for a semaphore here, so we only connect 1 socket at a time.
            // This is necessary for being able to see if connections can be combined
            try
            {
                await semaphoreSlim.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException tce)
            {
                return new CallResult<HighPerfUpdateSubscription>(new CancellationRequestedError(tce));
            }

            try
            {
                while (true)
                {
                    // Get a new or existing socket connection
                    var socketResult = await GetHighPerfSocketConnection<TUpdateType>(url, connectionFactory, ct).ConfigureAwait(false);
                    if (!socketResult)
                        return socketResult.As<HighPerfUpdateSubscription>(null);

                    socketConnection = socketResult.Data;

                    // Add a subscription on the socket connection
                    var success = socketConnection.AddSubscription(subscription);
                    if (!success)
                    {
                        _logger.FailedToAddSubscriptionRetryOnDifferentConnection(socketConnection.SocketId);
                        continue;
                    }

                    if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                    {
                        // Only 1 subscription per connection, so no need to wait for connection since a new subscription will create a new connection anyway
                        semaphoreSlim.Release();
                        released = true;
                    }

                    var needsConnecting = !socketConnection.Connected;

                    var connectResult = await ConnectIfNeededAsync(socketConnection, false, ct).ConfigureAwait(false);
                    if (!connectResult)
                        return new CallResult<HighPerfUpdateSubscription>(connectResult.Error!);

                    break;
                }
            }
            finally
            {
                if (!released)
                    semaphoreSlim.Release();
            }

            var subRequest = subscription.CreateSubscriptionQuery(socketConnection);
            if (subRequest != null)
            {
                // Send the request and wait for answer
                var sendResult = await socketConnection.SendAsync(subRequest).ConfigureAwait(false);
                if (!sendResult)
                {
                    await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                    return new CallResult<HighPerfUpdateSubscription>(sendResult.Error!);                    
                }
            }

            if (ct != default)
            {
                subscription.CancellationTokenRegistration = ct.Register(async () =>
                {
                    _logger.CancellationTokenSetClosingSubscription(socketConnection.SocketId, subscription.Id);
                    await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                }, false);
            }

            _logger.SubscriptionCompletedSuccessfully(socketConnection.SocketId, subscription.Id);
            return new CallResult<HighPerfUpdateSubscription>(new HighPerfUpdateSubscription(socketConnection, subscription));
        }

        /// <summary>
        /// Send a query on a socket connection to the BaseAddress and wait for the response
        /// </summary>
        /// <typeparam name="THandlerResponse">Expected result type</typeparam>
        /// <param name="query">The query</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected virtual Task<CallResult<THandlerResponse>> QueryAsync<THandlerResponse>(Query<THandlerResponse> query, CancellationToken ct = default)
        {
            return QueryAsync(BaseAddress, query, ct);
        }

        /// <summary>
        /// Send a query on a socket connection and wait for the response
        /// </summary>
        /// <typeparam name="THandlerResponse">Expected result type</typeparam>
        /// <param name="url">The url for the request</param>
        /// <param name="query">The query</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<THandlerResponse>> QueryAsync<THandlerResponse>(string url, Query<THandlerResponse> query, CancellationToken ct = default)
        {
            if (_disposing)
                return new CallResult<THandlerResponse>(new InvalidOperationError("Client disposed, can't query"));

            if (ct.IsCancellationRequested)
                return new CallResult<THandlerResponse>(new CancellationRequestedError());

            SocketConnection socketConnection;
            var released = false;
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                var socketResult = await GetSocketConnection(url, query.Authenticated, true, ct).ConfigureAwait(false);
                if (!socketResult)
                    return socketResult.As<THandlerResponse>(default);

                socketConnection = socketResult.Data;

                if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                {
                    // Can release early when only a single sub per connection
                    semaphoreSlim.Release();
                    released = true;
                }

                var connectResult = await ConnectIfNeededAsync(socketConnection, query.Authenticated, ct).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult<THandlerResponse>(connectResult.Error!);
            }
            finally
            {
                if (!released)
                    semaphoreSlim.Release();
            }

            if (socketConnection.PausedActivity)
            {
                _logger.HasBeenPausedCantSendQueryAtThisMoment(socketConnection.SocketId);
                return new CallResult<THandlerResponse>(new ServerError(new ErrorInfo(ErrorType.WebsocketPaused, "Socket is paused")));
            }

            if (ct.IsCancellationRequested)
                return new CallResult<THandlerResponse>(new CancellationRequestedError());

            return await socketConnection.SendAndWaitQueryAsync(query, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if a socket needs to be connected and does so if needed. Also authenticates on the socket if needed
        /// </summary>
        /// <param name="socket">The connection to check</param>
        /// <param name="authenticated">Whether the socket should authenticated</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<CallResult> ConnectIfNeededAsync(ISocketConnection socket, bool authenticated, CancellationToken ct)
        {
            if (socket.Connected)
                return CallResult.SuccessResult;

            var connectResult = await ConnectSocketAsync(socket, ct).ConfigureAwait(false);
            if (!connectResult)
                return connectResult;

            if (ClientOptions.DelayAfterConnect != TimeSpan.Zero)
                await Task.Delay(ClientOptions.DelayAfterConnect).ConfigureAwait(false);

            if (!authenticated || socket.Authenticated)
                return CallResult.SuccessResult;

            if (socket is not SocketConnection sc)
                throw new InvalidOperationException("HighPerfSocketConnection not supported for authentication");

            var result = await AuthenticateSocketAsync(sc).ConfigureAwait(false);
            if (!result)
                await socket.CloseAsync().ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Authenticate a socket connection
        /// </summary>
        /// <param name="socket">Socket to authenticate</param>
        /// <returns></returns>
        public virtual async Task<CallResult> AuthenticateSocketAsync(SocketConnection socket)
        {
            if (AuthenticationProvider == null)
                return new CallResult(new NoApiCredentialsError());

            _logger.AttemptingToAuthenticate(socket.SocketId);
            var authRequest = await GetAuthenticationRequestAsync(socket).ConfigureAwait(false);
            if (authRequest != null)
            {
                var result = await socket.SendAndWaitQueryAsync(authRequest).ConfigureAwait(false);

                if (!result)
                {
                    _logger.AuthenticationFailed(socket.SocketId);
                    if (socket.Connected)
                        await socket.CloseAsync().ConfigureAwait(false);

                    result.Error!.Message = "Authentication failed: " + result.Error.Message;
                    return new CallResult(result.Error)!;
                }

                _logger.Authenticated(socket.SocketId);
            }

            socket.Authenticated = true;
            return CallResult.SuccessResult;
        }

        /// <summary>
        /// Should return the request which can be used to authenticate a socket connection
        /// </summary>
        /// <returns></returns>
        protected internal virtual Task<Query?> GetAuthenticationRequestAsync(SocketConnection connection) => throw new NotImplementedException();

        /// <summary>
        /// Adds a system subscription. Used for example to reply to ping requests
        /// </summary>
        /// <param name="systemSubscription">The subscription</param>
        protected void AddSystemSubscription(SystemSubscription systemSubscription)
        {
            systemSubscriptions.Add(systemSubscription);
            foreach (var connection in socketConnections.Values)
                connection.AddSubscription(systemSubscription);
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
        protected internal virtual Task<Uri?> GetReconnectUriAsync(ISocketConnection connection)
        {
            return Task.FromResult<Uri?>(connection.ConnectionUri);
        }

        /// <summary>
        /// Update the subscription when the connection is restored after disconnecting. Can be used to update an authentication token for example.
        /// </summary>
        /// <param name="subscription">The subscription</param>
        /// <returns></returns>
        protected internal virtual Task<CallResult> RevitalizeRequestAsync(Subscription subscription)
        {
            return Task.FromResult(CallResult.SuccessResult);
        }

        /// <summary>
        /// Gets a connection for a new subscription or query. Can be an existing if there are open position or a new one.
        /// </summary>
        /// <param name="address">The address the socket is for</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <param name="dedicatedRequestConnection">Whether a dedicated request connection should be returned</param>
        /// <param name="ct">Cancellation token</param>
        /// <param name="topic">The subscription topic, can be provided when multiple of the same topics are not allowed on a connection</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<SocketConnection>> GetSocketConnection(string address, bool authenticated, bool dedicatedRequestConnection, CancellationToken ct, string? topic = null)
        {
            var socketQuery = socketConnections.Where(s => s.Value.Tag.TrimEnd('/') == address.TrimEnd('/')
                                                      && s.Value.ApiClient.GetType() == GetType()
                                                      && (AllowTopicsOnTheSameConnection || !s.Value.Topics.Contains(topic)))
                                                .Select(x => x.Value)
                                                .ToList();

            // If all current socket connections are reconnecting or resubscribing wait for that to finish as we can probably use the existing connection
            var delayStart = DateTime.UtcNow;
            var delayed = false;
            while (socketQuery.Count >= 1 && socketQuery.All(x => x.Status == SocketStatus.Reconnecting || x.Status == SocketStatus.Resubscribing))
            {
                if (DateTime.UtcNow - delayStart > TimeSpan.FromSeconds(10))
                {
                    if (socketQuery.Count >= 1 && socketQuery.All(x => x.Status == SocketStatus.Reconnecting || x.Status == SocketStatus.Resubscribing))
                    {
                        // If after this time we still trying to reconnect/reprocess there is some issue in the connection
                        _logger.TimeoutWaitingForReconnectingSocket();
                        return new CallResult<SocketConnection>(new CantConnectError());
                    }

                    break;
                }

                delayed = true;
                try { await Task.Delay(50, ct).ConfigureAwait(false); } catch (Exception) { }

                if (ct.IsCancellationRequested)
                    return new CallResult<SocketConnection>(new CancellationRequestedError());
            }

            if (delayed)
                _logger.WaitedForReconnectingSocket((long)(DateTime.UtcNow - delayStart).TotalMilliseconds);

            socketQuery = socketQuery.Where(s => (s.Status == SocketStatus.None || s.Status == SocketStatus.Connected)                                                     
                                                && (s.Authenticated == authenticated || !authenticated)
                                                && s.Connected).ToList();

            SocketConnection? connection;
            if (!dedicatedRequestConnection)
            {
                connection = socketQuery.Where(s => !s.DedicatedRequestConnection.IsDedicatedRequestConnection).OrderBy(s => s.UserSubscriptionCount).FirstOrDefault();
            }
            else
            {
                connection = socketQuery.Where(s => s.DedicatedRequestConnection.IsDedicatedRequestConnection).FirstOrDefault();
                if (connection != null && !connection.DedicatedRequestConnection.Authenticated)
                    // Mark dedicated request connection as authenticated if the request is authenticated
                    connection.DedicatedRequestConnection.Authenticated = authenticated;
            }

            if (connection != null)
            {
                if (connection.UserSubscriptionCount < ClientOptions.SocketSubscriptionsCombineTarget
                    || (socketConnections.Count >= (ApiOptions.MaxSocketConnections ?? ClientOptions.MaxSocketConnections) && socketConnections.All(s => s.Value.UserSubscriptionCount >= ClientOptions.SocketSubscriptionsCombineTarget)))
                {
                    // Use existing socket if it has less than target connections OR it has the least connections and we can't make new
                    return new CallResult<SocketConnection>(connection);
                }
            }

            var connectionAddress = await GetConnectionUrlAsync(address, authenticated).ConfigureAwait(false);
            if (!connectionAddress)
            {
                _logger.FailedToDetermineConnectionUrl(connectionAddress.Error?.ToString());
                return connectionAddress.As<SocketConnection>(null);
            }

            if (connectionAddress.Data != address)
                _logger.ConnectionAddressSetTo(connectionAddress.Data!);

            // Create new socket connection
            var socketConnection = new SocketConnection(_logger, SocketFactory, GetWebSocketParameters(connectionAddress.Data!), this, address);
            socketConnection.UnhandledMessage += HandleUnhandledMessage;
            socketConnection.ConnectRateLimitedAsync += HandleConnectRateLimitedAsync;
            if (dedicatedRequestConnection)
            {
                socketConnection.DedicatedRequestConnection = new DedicatedConnectionState
                {
                    IsDedicatedRequestConnection = dedicatedRequestConnection,
                    Authenticated = authenticated
                };
            }

            foreach (var ptg in PeriodicTaskRegistrations)
                socketConnection.QueryPeriodic(ptg.Identifier, ptg.Interval, ptg.QueryDelegate, ptg.Callback);

            foreach (var systemSubscription in systemSubscriptions)
                socketConnection.AddSubscription(systemSubscription);

            return new CallResult<SocketConnection>(socketConnection);
        }


        /// <summary>
        /// Gets a connection for a new subscription or query. Can be an existing if there are open position or a new one.
        /// </summary>
        /// <param name="address">The address the socket is for</param>
        /// <param name="connectionFactory">The factory for creating a socket connection</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<HighPerfSocketConnection<TUpdateType>>> GetHighPerfSocketConnection<TUpdateType>(
            string address,
            IHighPerfConnectionFactory connectionFactory,
            CancellationToken ct)
        {
            var socketQuery = highPerfSocketConnections.Where(s => s.Value.Tag.TrimEnd('/') == address.TrimEnd('/')
                                                      && s.Value.ApiClient.GetType() == GetType()
                                                      && s.Value.UpdateType == typeof(TUpdateType))
                                                .Select(x => x.Value)
                                                .ToList();


            socketQuery = socketQuery.Where(s => (s.Status == SocketStatus.None || s.Status == SocketStatus.Connected)
                                                && s.Connected).ToList();

            var connection = socketQuery.OrderBy(s => s.UserSubscriptionCount).FirstOrDefault();
            if (connection != null)
            {
                if (connection.UserSubscriptionCount < ClientOptions.SocketSubscriptionsCombineTarget
                    || (socketConnections.Count >= (ApiOptions.MaxSocketConnections ?? ClientOptions.MaxSocketConnections) && socketConnections.All(s => s.Value.UserSubscriptionCount >= ClientOptions.SocketSubscriptionsCombineTarget)))
                {
                    // Use existing socket if it has less than target connections OR it has the least connections and we can't make new
                    return new CallResult<HighPerfSocketConnection<TUpdateType>>((HighPerfSocketConnection<TUpdateType>)connection);
                }
            }

            var connectionAddress = await GetConnectionUrlAsync(address, false).ConfigureAwait(false);
            if (!connectionAddress)
            {
                _logger.FailedToDetermineConnectionUrl(connectionAddress.Error?.ToString());
                return connectionAddress.As<HighPerfSocketConnection<TUpdateType>>(null);
            }

            if (connectionAddress.Data != address)
                _logger.ConnectionAddressSetTo(connectionAddress.Data!);

            // Create new socket connection
            var socketConnection = connectionFactory.CreateHighPerfConnection<TUpdateType>(_logger, SocketFactory, GetWebSocketParameters(connectionAddress.Data!), this, address);
            foreach (var ptg in PeriodicTaskRegistrations)
                socketConnection.QueryPeriodic(ptg.Identifier, ptg.Interval, (con) => ptg.QueryDelegate(con).Request);

            return new CallResult<HighPerfSocketConnection<TUpdateType>>(socketConnection);
        }


        /// <summary>
        /// Process an unhandled message
        /// </summary>
        /// <param name="message">The message that wasn't processed</param>
        protected virtual void HandleUnhandledMessage(IMessageAccessor message)
        {
        }

        /// <summary>
        /// Process connect rate limited
        /// </summary>
        protected async virtual Task HandleConnectRateLimitedAsync()
        {
            if (ClientOptions.RateLimiterEnabled && ClientOptions.ConnectDelayAfterRateLimited.HasValue)
            {
                var retryAfter = DateTime.UtcNow.Add(ClientOptions.ConnectDelayAfterRateLimited.Value);
                _logger.AddingRetryAfterGuard(retryAfter);
                RateLimiter ??= new RateLimitGate("Connection");
                await RateLimiter.SetRetryAfterGuardAsync(retryAfter, RateLimitItemType.Connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Connect a socket
        /// </summary>
        /// <param name="socketConnection">The socket to connect</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<CallResult> ConnectSocketAsync(ISocketConnection socketConnection, CancellationToken ct)
        {
            var connectResult = await socketConnection.ConnectAsync(ct).ConfigureAwait(false);
            if (connectResult)
            {
                if (socketConnection is SocketConnection sc)
                    socketConnections.TryAdd(socketConnection.SocketId, sc);
                else if (socketConnection is HighPerfSocketConnection hsc)
                    highPerfSocketConnections.TryAdd(socketConnection.SocketId, hsc);
                return connectResult;
            }

            socketConnection.Dispose();
            return connectResult;
        }

        /// <summary>
        /// Get parameters for the websocket connection
        /// </summary>
        /// <param name="address">The address to connect to</param>
        /// <returns></returns>
        protected virtual WebSocketParameters GetWebSocketParameters(string address)
            => new(new Uri(address), ClientOptions.ReconnectPolicy)
            {
                KeepAliveInterval = KeepAliveInterval,
                KeepAliveTimeout = KeepAliveTimeout,
                ReconnectInterval = ClientOptions.ReconnectInterval,
                RateLimiter = ClientOptions.RateLimiterEnabled ? RateLimiter : null,
                RateLimitingBehavior = ClientOptions.RateLimitingBehaviour,
                Proxy = ClientOptions.Proxy,
                Timeout = ApiOptions.SocketNoDataTimeout ?? ClientOptions.SocketNoDataTimeout,
                ReceiveBufferSize = ClientOptions.ReceiveBufferSize,
                UseUpdatedDeserialization = ClientOptions.UseUpdatedDeserialization
            };

        ///// <summary>
        ///// Create a socket for an address
        ///// </summary>
        ///// <param name="address">The address the socket should connect to</param>
        ///// <returns></returns>
        //protected internal virtual IWebsocket CreateSocket(string address)
        //{
        //    var socket = SocketFactory.CreateWebsocket(_logger,  GetWebSocketParameters(address));
        //    _logger.SocketCreatedForAddress(socket.Id, address);
        //    return socket;
        //}

        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task<bool> UnsubscribeAsync(int subscriptionId)
        {
            Subscription? subscription = null;
            SocketConnection? connection = null;
            foreach (var socket in socketConnections.Values.ToList())
            {
                subscription = socket.GetSubscription(subscriptionId);
                if (subscription != null)
                {
                    connection = socket;
                    break;
                }
            }

            if (subscription == null || connection == null)
                return false;

            _logger.UnsubscribingSubscription(connection.SocketId, subscriptionId);
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

            _logger.UnsubscribingSubscription(subscription.SocketId, subscription.Id);
            await subscription.CloseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAllAsync()
        {
            var sum = socketConnections.Sum(s => s.Value.UserSubscriptionCount);
            if (sum == 0)
                return;

            _logger.UnsubscribingAll(socketConnections.Sum(s => s.Value.UserSubscriptionCount));
            var tasks = new List<Task>();
            {
                var socketList = socketConnections.Values;
                foreach (var connection in socketList)
                {
                    foreach(var subscription in connection.Subscriptions.Where(x => x.UserSubscription))
                        tasks.Add(connection.CloseAsync(subscription));
                }
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Reconnect all connections
        /// </summary>
        /// <returns></returns>
        public virtual async Task ReconnectAsync()
        {
            _logger.ReconnectingAllConnections(socketConnections.Count);
            var tasks = new List<Task>();
            {
                var socketList = socketConnections.Values;
                foreach (var sub in socketList)
                    tasks.Add(sub.TriggerReconnectAsync());
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<CallResult> PrepareConnectionsAsync()
        {
            foreach (var item in DedicatedConnectionConfigs)
            {
                var socketResult = await GetSocketConnection(item.SocketAddress, item.Authenticated, true, CancellationToken.None).ConfigureAwait(false);
                if (!socketResult)
                    return socketResult.AsDataless();

                var connectResult = await ConnectIfNeededAsync(socketResult.Data, item.Authenticated, default).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult(connectResult.Error!);
            }

            return CallResult.SuccessResult;
        }

        /// <inheritdoc />
        public override void SetOptions<T>(UpdateOptions<T> options)
        {
            var previousProxyIsSet = ClientOptions.Proxy != null;
            base.SetOptions(options);

            if ((!previousProxyIsSet && options.Proxy == null)
                || socketConnections.IsEmpty)
            {
                return;
            }

            _logger.LogInformation("Reconnecting websockets to apply proxy");

            // Update proxy, also triggers reconnect
            foreach (var connection in socketConnections)
                _ = connection.Value.UpdateProxy(options.Proxy);
        }

        /// <summary>
        /// Log the current state of connections and subscriptions
        /// </summary>
        public string GetSubscriptionsState(bool includeSubDetails = true)
        {
            return GetState(includeSubDetails).ToString();
        }

        /// <summary>
        /// Gets the state of the client
        /// </summary>
        /// <param name="includeSubDetails">True to get details for each subscription</param>
        /// <returns></returns>
        public SocketApiClientState GetState(bool includeSubDetails = true)
        {
            var connectionStates = new List<SocketConnectionState>();
            foreach (var socketIdAndConnection in socketConnections)
            {
                SocketConnection connection = socketIdAndConnection.Value;
                SocketConnectionState connectionState = connection.GetState(includeSubDetails);
                connectionStates.Add(connectionState);
            }

            return new SocketApiClientState(socketConnections.Count, CurrentSubscriptions, IncomingKbps, connectionStates);
        }

        /// <summary>
        /// Get the current state of the client
        /// </summary>
        /// <param name="Connections">Number of sockets for this client</param>
        /// <param name="Subscriptions">Total number of subscriptions</param>
        /// <param name="DownloadSpeed">Total download speed</param>
        /// <param name="ConnectionStates">State of each socket connection</param>
        public record SocketApiClientState(
            int Connections,
            int Subscriptions,
            double DownloadSpeed,
            List<SocketConnectionState> ConnectionStates)
        {
            /// <summary>
            /// Print the state of the client
            /// </summary>
            /// <param name="sb"></param>
            /// <returns></returns>
            protected virtual bool PrintMembers(StringBuilder sb)
            {
                sb.AppendLine();
                sb.AppendLine($"\tTotal connections: {Connections}");
                sb.AppendLine($"\tTotal subscriptions: {Subscriptions}");
                sb.AppendLine($"\tDownload speed: {DownloadSpeed} kbps");
                sb.AppendLine($"\tConnections:");
                ConnectionStates.ForEach(cs =>
                {
                    sb.AppendLine($"\t\tId: {cs.Id}");
                    sb.AppendLine($"\t\tAddress: {cs.Address}");
                    sb.AppendLine($"\t\tTotal subscriptions: {cs.Subscriptions}");
                    sb.AppendLine($"\t\tStatus: {cs.Status}");
                    sb.AppendLine($"\t\tAuthenticated: {cs.Authenticated}");
                    sb.AppendLine($"\t\tDownload speed: {cs.DownloadSpeed} kbps");
                    sb.AppendLine($"\t\tPending queries: {cs.PendingQueries}");
                    if (cs.SubscriptionStates?.Count > 0)
                    {
                        sb.AppendLine($"\t\tSubscriptions:");
                        cs.SubscriptionStates.ForEach(subState =>
                        {
                            sb.AppendLine($"\t\t\tId: {subState.Id}");
                            sb.AppendLine($"\t\t\tStatus: {subState.Status}");
                            sb.AppendLine($"\t\t\tInvocations: {subState.Invocations}");
                            sb.AppendLine($"\t\t\tIdentifiers: [{subState.ListenMatcher.ToString()}]");
                        });
                    }
                });

                return true;
            }
        }

        /// <summary>
        /// Dispose the client
        /// </summary>
        public override void Dispose()
        {
            _disposing = true;
            var tasks = new List<Task>();
            {
                var socketList = socketConnections.Values.Where(x => x.UserSubscriptionCount > 0 || x.Connected);
                if (socketList.Any())
                    _logger.DisposingSocketClient();

                foreach (var connection in socketList)
                {
                    tasks.Add(connection.CloseAsync());
                }
            }

            semaphoreSlim?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Get the listener identifier for the message
        /// </summary>
        /// <param name="messageAccessor"></param>
        /// <returns></returns>
        public abstract string? GetListenerIdentifier(IMessageAccessor messageAccessor);

        /// <summary>
        /// Preprocess a stream message
        /// </summary>
        public virtual ReadOnlySpan<byte> PreprocessStreamMessage(SocketConnection connection, WebSocketMessageType type, ReadOnlySpan<byte> data) => data;
        /// <summary>
        /// Preprocess a stream message
        /// </summary>
        public virtual ReadOnlyMemory<byte> PreprocessStreamMessage(SocketConnection connection, WebSocketMessageType type, ReadOnlyMemory<byte> data) => data;

        /// <summary>
        /// Create a new message converter instance
        /// </summary>
        /// <returns></returns>
        public abstract IMessageConverter CreateMessageConverter(WebSocketMessageType messageType);
    }
}
