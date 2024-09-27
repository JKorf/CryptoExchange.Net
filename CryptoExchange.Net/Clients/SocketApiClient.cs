using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.Sockets;
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
        /// Semaphore used while creating sockets
        /// </summary>
        protected internal readonly SemaphoreSlim semaphoreSlim = new(1);

        /// <summary>
        /// Keep alive interval for websocket connection
        /// </summary>
        protected TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(10);

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
        protected internal virtual IByteMessageAccessor CreateAccessor() => new JsonNetByteMessageAccessor();

        /// <summary>
        /// Create a serializer instance
        /// </summary>
        /// <returns></returns>
        protected internal virtual IMessageSerializer CreateSerializer() => new JsonNetMessageSerializer();

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
        protected virtual void RegisterPeriodicQuery(string identifier, TimeSpan interval, Func<SocketConnection, Query> queryDelegate, Action<CallResult>? callback)
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
            catch (OperationCanceledException)
            {
                return new CallResult<UpdateSubscription>(new CancellationRequestedError());
            }

            try
            {
                while (true)
                {
                    // Get a new or existing socket connection
                    var socketResult = await GetSocketConnection(url, subscription.Authenticated, false).ConfigureAwait(false);
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
                _logger.HasBeenPausedCantSubscribeAtThisMoment(socketConnection.SocketId);
                return new CallResult<UpdateSubscription>(new ServerError("Socket is paused"));
            }

            var waitEvent = new AsyncResetEvent(false);
            var subQuery = subscription.GetSubQuery(socketConnection);
            if (subQuery != null)
            {
                // Send the request and wait for answer
                var subResult = await socketConnection.SendAndWaitQueryAsync(subQuery, waitEvent).ConfigureAwait(false);
                if (!subResult)
                {
                    waitEvent?.Set();
                    var isTimeout = subResult.Error is CancellationRequestedError;
                    if (isTimeout && subscription.Confirmed)
                    {
                        // No response received, but the subscription did receive updates. We'll assume success
                    }
                    else
                    {
                        _logger.FailedToSubscribe(socketConnection.SocketId, subResult.Error?.ToString());
                        // If this was a timeout we still need to send an unsubscribe to prevent messages coming in later
                        await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                        return new CallResult<UpdateSubscription>(subResult.Error!);
                    }
                }

                subscription.HandleSubQueryResponse(subQuery.Response!);
            }

            subscription.Confirmed = true;
            if (ct != default)
            {
                subscription.CancellationTokenRegistration = ct.Register(async () =>
                {
                    _logger.CancellationTokenSetClosingSubscription(socketConnection.SocketId, subscription.Id);
                    await socketConnection.CloseAsync(subscription).ConfigureAwait(false);
                }, false);
            }

            waitEvent?.Set();
            _logger.SubscriptionCompletedSuccessfully(socketConnection.SocketId, subscription.Id);
            return new CallResult<UpdateSubscription>(new UpdateSubscription(socketConnection, subscription));
        }

        /// <summary>
        /// Send a query on a socket connection to the BaseAddress and wait for the response
        /// </summary>
        /// <typeparam name="THandlerResponse">Expected result type</typeparam>
        /// <typeparam name="TServerResponse">The type returned to the caller</typeparam>
        /// <param name="query">The query</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected virtual Task<CallResult<THandlerResponse>> QueryAsync<TServerResponse, THandlerResponse>(Query<TServerResponse, THandlerResponse> query, CancellationToken ct = default)
        {
            return QueryAsync(BaseAddress, query, ct);
        }

        /// <summary>
        /// Send a query on a socket connection and wait for the response
        /// </summary>
        /// <typeparam name="THandlerResponse">Expected result type</typeparam>
        /// <typeparam name="TServerResponse">The type returned to the caller</typeparam>
        /// <param name="url">The url for the request</param>
        /// <param name="query">The query</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<THandlerResponse>> QueryAsync<TServerResponse, THandlerResponse>(string url, Query<TServerResponse, THandlerResponse> query, CancellationToken ct = default)
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
                var socketResult = await GetSocketConnection(url, query.Authenticated, true).ConfigureAwait(false);
                if (!socketResult)
                    return socketResult.As<THandlerResponse>(default);

                socketConnection = socketResult.Data;

                if (ClientOptions.SocketSubscriptionsCombineTarget == 1)
                {
                    // Can release early when only a single sub per connection
                    semaphoreSlim.Release();
                    released = true;
                }

                var connectResult = await ConnectIfNeededAsync(socketConnection, query.Authenticated).ConfigureAwait(false);
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
                return new CallResult<THandlerResponse>(new ServerError("Socket is paused"));
            }

            if (ct.IsCancellationRequested)
                return new CallResult<THandlerResponse>(new CancellationRequestedError());

            return await socketConnection.SendAndWaitQueryAsync(query, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if a socket needs to be connected and does so if needed. Also authenticates on the socket if needed
        /// </summary>
        /// <param name="socket">The connection to check</param>
        /// <param name="authenticated">Whether the socket should authenticated</param>
        /// <returns></returns>
        protected virtual async Task<CallResult> ConnectIfNeededAsync(SocketConnection socket, bool authenticated)
        {
            if (socket.Connected)
                return new CallResult(null);

            var connectResult = await ConnectSocketAsync(socket).ConfigureAwait(false);
            if (!connectResult)
                return connectResult;

            if (ClientOptions.DelayAfterConnect != TimeSpan.Zero)
                await Task.Delay(ClientOptions.DelayAfterConnect).ConfigureAwait(false);

            if (!authenticated || socket.Authenticated)
                return new CallResult(null);

            var result = await AuthenticateSocketAsync(socket).ConfigureAwait(false);
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
            var authRequest = GetAuthenticationRequest(socket);
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
            }

            _logger.Authenticated(socket.SocketId);
            socket.Authenticated = true;
            return new CallResult(null);
        }

        /// <summary>
        /// Should return the request which can be used to authenticate a socket connection
        /// </summary>
        /// <returns></returns>
        protected internal virtual Query? GetAuthenticationRequest(SocketConnection connection) => throw new NotImplementedException();

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
        protected internal virtual Task<Uri?> GetReconnectUriAsync(SocketConnection connection)
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
            return Task.FromResult(new CallResult(null));
        }

        /// <summary>
        /// Gets a connection for a new subscription or query. Can be an existing if there are open position or a new one.
        /// </summary>
        /// <param name="address">The address the socket is for</param>
        /// <param name="authenticated">Whether the socket should be authenticated</param>
        /// <param name="dedicatedRequestConnection">Whether a dedicated request connection should be returned</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<SocketConnection>> GetSocketConnection(string address, bool authenticated, bool dedicatedRequestConnection)
        {
            var socketQuery = socketConnections.Where(s => (s.Value.Status == SocketConnection.SocketStatus.None || s.Value.Status == SocketConnection.SocketStatus.Connected)
                                                      && s.Value.Tag.TrimEnd('/') == address.TrimEnd('/')
                                                      && s.Value.ApiClient.GetType() == GetType()
                                                      && (s.Value.Authenticated == authenticated || !authenticated)
                                                      && s.Value.Connected);

            SocketConnection connection;
            if (!dedicatedRequestConnection)
            {
                connection = socketQuery.Where(s => !s.Value.DedicatedRequestConnection).OrderBy(s => s.Value.UserSubscriptionCount).FirstOrDefault().Value;
            }
            else
            {
                connection = socketQuery.Where(s => s.Value.DedicatedRequestConnection).FirstOrDefault().Value;
            }

            if (connection != null)
            {
                if (connection.UserSubscriptionCount < ClientOptions.SocketSubscriptionsCombineTarget || socketConnections.Count >= (ApiOptions.MaxSocketConnections ?? ClientOptions.MaxSocketConnections) && socketConnections.All(s => s.Value.UserSubscriptionCount >= ClientOptions.SocketSubscriptionsCombineTarget))
                    // Use existing socket if it has less than target connections OR it has the least connections and we can't make new
                    return new CallResult<SocketConnection>(connection);
            }

            var connectionAddress = await GetConnectionUrlAsync(address, authenticated).ConfigureAwait(false);
            if (!connectionAddress)
            {
                _logger.FailedToDetermineConnectionUrl(connectionAddress.Error?.ToString());
                return connectionAddress.As<SocketConnection>(null);
            }

            if (connectionAddress.Data != address)
                _logger.ConnectionAddressSetTo(connectionAddress.Data!);

            // Create new socket
            var socket = CreateSocket(connectionAddress.Data!);
            var socketConnection = new SocketConnection(_logger, this, socket, address);
            socketConnection.UnhandledMessage += HandleUnhandledMessage;
            socketConnection.ConnectRateLimitedAsync += HandleConnectRateLimitedAsync;
            socketConnection.DedicatedRequestConnection = dedicatedRequestConnection;

            foreach (var ptg in PeriodicTaskRegistrations)
                socketConnection.QueryPeriodic(ptg.Identifier, ptg.Interval, ptg.QueryDelegate, ptg.Callback);

            foreach (var systemSubscription in systemSubscriptions)
                socketConnection.AddSubscription(systemSubscription);

            return new CallResult<SocketConnection>(socketConnection);
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
            if (ClientOptions.RateLimiterEnabled && RateLimiter is not null && ClientOptions.ConnectDelayAfterRateLimited is not null)
            {
                var retryAfter = DateTime.UtcNow.Add(ClientOptions.ConnectDelayAfterRateLimited.Value);
                _logger.AddingRetryAfterGuard(retryAfter);
                await RateLimiter.SetRetryAfterGuardAsync(retryAfter, RateLimiting.RateLimitItemType.Connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Connect a socket
        /// </summary>
        /// <param name="socketConnection">The socket to connect</param>
        /// <returns></returns>
        protected virtual async Task<CallResult> ConnectSocketAsync(SocketConnection socketConnection)
        {
            var connectResult = await socketConnection.ConnectAsync().ConfigureAwait(false);
            if (connectResult)
            {
                socketConnections.TryAdd(socketConnection.SocketId, socketConnection);
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
                ReconnectInterval = ClientOptions.ReconnectInterval,
                RateLimiter = ClientOptions.RateLimiterEnabled ? RateLimiter : null,
                RateLimitingBehaviour = ClientOptions.RateLimitingBehaviour,
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
            _logger.SocketCreatedForAddress(socket.Id, address);
            return socket;
        }

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
                foreach (var connection in socketList.Where(s => !s.DedicatedRequestConnection))
                    tasks.Add(connection.CloseAsync());
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
                var socketResult = await GetSocketConnection(item.SocketAddress, item.Authenticated, true).ConfigureAwait(false);
                if (!socketResult)
                    return socketResult.AsDataless();

                var connectResult = await ConnectIfNeededAsync(socketResult.Data, item.Authenticated).ConfigureAwait(false);
                if (!connectResult)
                    return new CallResult(connectResult.Error!);
            }

            return new CallResult(null);
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
            var connectionStates = new List<SocketConnection.SocketConnectionState>();
            foreach (var socketIdAndConnection in socketConnections)
            {
                SocketConnection connection = socketIdAndConnection.Value;
                SocketConnection.SocketConnectionState connectionState = connection.GetState(includeSubDetails);
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
            List<SocketConnection.SocketConnectionState> ConnectionStates)
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
                            sb.AppendLine($"\t\t\tConfirmed: {subState.Confirmed}");
                            sb.AppendLine($"\t\t\tInvocations: {subState.Invocations}");
                            sb.AppendLine($"\t\t\tIdentifiers: [{string.Join(",", subState.Identifiers)}]");
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
        /// <param name="connection"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual ReadOnlyMemory<byte> PreprocessStreamMessage(SocketConnection connection, WebSocketMessageType type, ReadOnlyMemory<byte> data) => data;
    }
}
