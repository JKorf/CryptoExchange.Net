using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// State of a the connection
    /// </summary>
    /// <param name="Id">The id of the socket connection</param>
    /// <param name="Address">The connection URI</param>
    /// <param name="Subscriptions">Number of subscriptions on this socket</param>
    /// <param name="Status">Socket status</param>
    /// <param name="Authenticated">If the connection is authenticated</param>
    /// <param name="DownloadSpeed">Download speed over this socket</param>
    /// <param name="PendingQueries">Number of non-completed queries</param>
    /// <param name="SubscriptionStates">State for each subscription on this socket</param>
    public record SocketConnectionState(
        int Id,
        string Address,
        int Subscriptions,
        SocketStatus Status,
        bool Authenticated,
        double DownloadSpeed,
        int PendingQueries,
        List<Subscription.SubscriptionState> SubscriptionStates
    );

    /// <summary>
    /// Status of the socket connection
    /// </summary>
    public enum SocketStatus
    {
        /// <summary>
        /// None/Initial
        /// </summary>
        None,
        /// <summary>
        /// Connected
        /// </summary>
        Connected,
        /// <summary>
        /// Reconnecting
        /// </summary>
        Reconnecting,
        /// <summary>
        /// Resubscribing on reconnected socket
        /// </summary>
        Resubscribing,
        /// <summary>
        /// Closing
        /// </summary>
        Closing,
        /// <summary>
        /// Closed
        /// </summary>
        Closed,
        /// <summary>
        /// Disposed
        /// </summary>
        Disposed
    }

    /// <summary>
    /// A single socket connection to the server
    /// </summary>
    public class SocketConnection : ISocketConnection
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
        /// Failed to resubscribe all subscription on the reconnected socket
        /// </summary>
        public event Action<Error>? ResubscribingFailed;

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
        public event Action<IMessageAccessor>? UnhandledMessage;

        /// <summary>
        /// Connection was rate limited and couldn't be established
        /// </summary>
        public Func<Task>? ConnectRateLimitedAsync;

        /// <summary>
        /// The amount of subscriptions on this connection
        /// </summary>
        public int UserSubscriptionCount
        {
            get
            {
                lock (_listenersLock)
                    return _listeners.OfType<Subscription>().Count(h => h.UserSubscription);
            }
        }

        /// <summary>
        /// Get a copy of the current message subscriptions
        /// </summary>
        public Subscription[] Subscriptions
        {
            get
            {
                lock (_listenersLock)
                    return _listeners.OfType<Subscription>().Where(h => h.UserSubscription).ToArray();
            }
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
        public Uri ConnectionUri => _socket.Uri;

        /// <summary>
        /// The API client the connection is for
        /// </summary>
        public SocketApiClient ApiClient { get; set; }

        /// <summary>
        /// Time of disconnecting
        /// </summary>
        public DateTime? DisconnectTime { get; set; }

        /// <summary>
        /// Tag for identification
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Additional properties for this connection
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// If activity is paused
        /// </summary>
        public bool PausedActivity
        {
            get => _pausedActivity;
            set
            {
                if (_pausedActivity != value)
                {
                    _pausedActivity = value;
                    _logger.ActivityPaused(SocketId, value);
                    if (_pausedActivity) _ = Task.Run(() => ActivityPaused?.Invoke());
                    else _ = Task.Run(() => ActivityUnpaused?.Invoke());
                }
            }
        }

        /// <summary>
        /// Status of the socket connection
        /// </summary>
        public SocketStatus Status
        {
            get => _status;
            private set
            {
                if (_status == value)
                    return;

                var oldStatus = _status;
                _status = value;
                _logger.SocketStatusChanged(SocketId, oldStatus, value);
            }
        }

        /// <summary>
        /// Info on whether this connection is a dedicated request connection
        /// </summary>
        public DedicatedConnectionState DedicatedRequestConnection { get; internal set; } = new DedicatedConnectionState();

        /// <summary>
        /// Current subscription topics on this connection
        /// </summary>
        public string[] Topics
        {
            get
            {
                lock (_listenersLock)
                    return _listeners.OfType<Subscription>().Select(x => x.Topic).Where(t => t != null).ToArray()!;
            }
        }

        /// <summary>
        /// The number of current pending requests
        /// </summary>
        public int PendingRequests
        {
            get
            {
                lock (_listenersLock)
                    return _listeners.OfType<Query>().Where(x => !x.Completed).Count();
            }
        }

        private bool _pausedActivity;
#if NET9_0_OR_GREATER
        private readonly Lock _listenersLock = new Lock();
#else
        private readonly object _listenersLock = new object();
#endif
        private readonly List<IMessageProcessor> _listeners;
        private readonly ILogger _logger;
        private SocketStatus _status;

        private readonly IMessageSerializer _serializer;
        private IByteMessageAccessor? _stringMessageAccessor;
        private IByteMessageAccessor? _byteMessageAccessor;

        private IMessageConverter? _messageConverter;

        /// <summary>
        /// The task that is sending periodic data on the websocket. Can be used for sending Ping messages every x seconds or similar. Not necessary.
        /// </summary>
        protected Task? periodicTask;

        /// <summary>
        /// Wait event for the periodicTask
        /// </summary>
        protected AsyncResetEvent? periodicEvent;

        /// <summary>
        /// The underlying websocket
        /// </summary>
        private readonly IWebsocket _socket;

        /// <summary>
        /// Cache for deserialization, only caches for a single message
        /// </summary>
        private readonly Dictionary<Type, object> _deserializationCache = new Dictionary<Type, object>();

        /// <summary>
        /// New socket connection
        /// </summary>
        public SocketConnection(ILogger logger, IWebsocketFactory socketFactory, WebSocketParameters parameters, SocketApiClient apiClient, string tag)
        {
            _logger = logger;
            ApiClient = apiClient;
            Tag = tag;
            Properties = new Dictionary<string, object>();

            _socket = socketFactory.CreateWebsocket(logger, this, parameters);
            _logger.SocketCreatedForAddress(_socket.Id, parameters.Uri.ToString());

            _socket.OnStreamMessage += HandleStreamMessage;
            _socket.OnRequestSent += HandleRequestSentAsync;
            _socket.OnRequestRateLimited += HandleRequestRateLimitedAsync;
            _socket.OnConnectRateLimited += HandleConnectRateLimitedAsync;
            _socket.OnOpen += HandleOpenAsync;
            _socket.OnClose += HandleCloseAsync;
            _socket.OnReconnecting += HandleReconnectingAsync;
            _socket.OnReconnected += HandleReconnectedAsync;
            _socket.OnError += HandleErrorAsync;
            _socket.GetReconnectionUrl = GetReconnectionUrlAsync;

            _listeners = new List<IMessageProcessor>();

            _serializer = apiClient.CreateSerializer();
        }

        /// <summary>
        /// Handler for a socket opening
        /// </summary>
        protected virtual Task HandleOpenAsync()
        {
            Status = SocketStatus.Connected;
            PausedActivity = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for a socket closing without reconnect
        /// </summary>
        protected virtual Task HandleCloseAsync()
        {
            Status = SocketStatus.Closed;
            Authenticated = false;

            if (ApiClient.socketConnections.ContainsKey(SocketId))
                ApiClient.socketConnections.TryRemove(SocketId, out _);

            lock (_listenersLock)
            {
                foreach (var subscription in _listeners.OfType<Subscription>().Where(l => l.UserSubscription && !l.IsClosingConnection))
                {
                    subscription.IsClosingConnection = true;
                    subscription.Reset();
                }

                foreach (var query in _listeners.OfType<Query>().ToList())
                {
                    query.Fail(new WebError("Connection interrupted"));
                    _listeners.Remove(query);
                }
            }

            _ = Task.Run(() => ConnectionClosed?.Invoke());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for a socket losing connection and starting reconnect
        /// </summary>
        protected virtual Task HandleReconnectingAsync()
        {
            Status = SocketStatus.Reconnecting;
            DisconnectTime = DateTime.UtcNow;
            Authenticated = false;

            lock (_listenersLock)
            {
                foreach (var subscription in _listeners.OfType<Subscription>().Where(l => l.UserSubscription))
                    subscription.Reset();

                foreach (var query in _listeners.OfType<Query>().ToList())
                {
                    query.Fail(new WebError("Connection interrupted"));
                    _listeners.Remove(query);
                }
            }

            _ = Task.Run(() => ConnectionLost?.Invoke());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the url to connect to when reconnecting
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<Uri?> GetReconnectionUrlAsync()
        {
            return await ApiClient.GetReconnectUriAsync(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Handler for a socket which has reconnected
        /// </summary>
        protected virtual Task HandleReconnectedAsync()
        {
            Status = SocketStatus.Resubscribing;

            lock (_listenersLock)
            {
                foreach (var query in _listeners.OfType<Query>().ToList())
                {
                    query.Fail(new WebError("Connection interrupted"));
                    _listeners.Remove(query);
                }
            }

            // Can't wait for this as it would cause a deadlock
            _ = Task.Run(async () =>
            {
                try
                {
                    var reconnectSuccessful = await ProcessReconnectAsync().ConfigureAwait(false);
                    if (!reconnectSuccessful)
                    {
                        _logger.FailedReconnectProcessing(SocketId, reconnectSuccessful.Error!.ToString());
                        _ = Task.Run(() => ResubscribingFailed?.Invoke(reconnectSuccessful.Error));
                        _ = _socket.ReconnectAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Status = SocketStatus.Connected;
                        _ = Task.Run(() =>
                        {
                            ConnectionRestored?.Invoke(DateTime.UtcNow - DisconnectTime!.Value);
                            DisconnectTime = null;
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.UnknownExceptionWhileProcessingReconnection(SocketId, ex);
                    _ = _socket.ReconnectAsync().ConfigureAwait(false);
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for an error on a websocket
        /// </summary>
        /// <param name="e">The exception</param>
        protected virtual Task HandleErrorAsync(Exception e)
        {
            if (e is WebSocketException wse)
                _logger.WebSocketErrorCodeAndDetails(SocketId, wse.WebSocketErrorCode, wse.Message, wse);
            else
                _logger.WebSocketError(SocketId, e.Message, e);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for whenever a request is rate limited and rate limit behavior is set to fail
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        protected virtual Task HandleRequestRateLimitedAsync(int requestId)
        {
            Query? query;
            lock (_listenersLock)
            {
                query = _listeners.OfType<Query>().FirstOrDefault(x => x.Id == requestId);
            }

            if (query == null)
                return Task.CompletedTask;

            query.Fail(new ClientRateLimitError("Connection rate limit reached"));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for whenever a connection was rate limited and couldn't be established
        /// </summary>
        /// <returns></returns>
        protected async virtual Task HandleConnectRateLimitedAsync()
        {
            if (ConnectRateLimitedAsync is not null)
                await ConnectRateLimitedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Handler for whenever a request is sent over the websocket
        /// </summary>
        /// <param name="requestId">Id of the request sent</param>
        protected virtual Task HandleRequestSentAsync(int requestId)
        {
            Query? query;
            lock (_listenersLock)
            {
                query = _listeners.OfType<Query>().FirstOrDefault(x => x.Id == requestId);
            }

            if (query == null)
                return Task.CompletedTask;

            query.IsSend(query.RequestTimeout ?? ApiClient.ClientOptions.RequestTimeout);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle a message
        /// </summary>
        protected internal virtual void HandleStreamMessage2(WebSocketMessageType type, ReadOnlySpan<byte> data)
        {
            //var sw = Stopwatch.StartNew();
            var receiveTime = DateTime.UtcNow;

            //// 1. Decrypt/Preprocess if necessary
            //data = ApiClient.PreprocessStreamMessage(this, type, data);

            _messageConverter ??= ApiClient.CreateMessageConverter();

            var messageType = _messageConverter.GetMessageType(data, type); 
            if (messageType.Type == null)
            {
                // Failed to determine message type
                return;
            }

            var result = _messageConverter.Deserialize(data, messageType.Type);
            if (result == null)
            {
                // Deserialize error
                return;
            }

            var targetType = messageType.Type;
            List<IMessageProcessor> listeners;
            lock (_listenersLock)
                listeners = _listeners.Where(x => x.DeserializationTypes.Contains(targetType)).ToList();
            if (listeners.Count == 0)
            {
                // No subscriptions found for type
                return;
            }

            var dataEvent = new DataEvent<object>(result, null, null, null /*originalData*/, receiveTime, null);
            foreach (var subscription in listeners)
            {
                var links = subscription.MessageMatcher.GetHandlerLinks(messageType.Identifier);
                foreach(var link in links)
                    subscription.Handle(this, dataEvent, link);
            }
        }

        /// <summary>
        /// Handle a message
        /// </summary>
        protected virtual async Task HandleStreamMessage(WebSocketMessageType type, ReadOnlyMemory<byte> data)
        {
            var sw = Stopwatch.StartNew();
            var receiveTime = DateTime.UtcNow;
            string? originalData = null;

            // 1. Decrypt/Preprocess if necessary
            data = ApiClient.PreprocessStreamMessage(this, type, data);

            // 2. Read data into accessor
            IByteMessageAccessor accessor;
            if (type == WebSocketMessageType.Binary)
                accessor = _stringMessageAccessor ??= ApiClient.CreateAccessor(type);
            else
                accessor = _byteMessageAccessor ??= ApiClient.CreateAccessor(type);

            var result = accessor.Read(data);
            try
            {
                bool outputOriginalData = ApiClient.ApiOptions.OutputOriginalData ?? ApiClient.ClientOptions.OutputOriginalData;
                if (outputOriginalData)
                {
                    originalData = accessor.GetOriginalString();
                    _logger.ReceivedData(SocketId, originalData);
                }

                if (!accessor.IsValid && !ApiClient.ProcessUnparsableMessages)
                {
                    _logger.FailedToParse(SocketId, result.Error!.Message ?? result.Error!.ErrorDescription!);
                    return;
                }

                // 3. Determine the identifying properties of this message
                var listenId = ApiClient.GetListenerIdentifier(accessor);
                if (listenId == null)
                {
                    originalData ??= "[OutputOriginalData is false]";
                    if (!ApiClient.UnhandledMessageExpected)
                        _logger.FailedToEvaluateMessage(SocketId, originalData);

                    UnhandledMessage?.Invoke(accessor);
                    return;
                }

                bool processed = false;
                var totalUserTime = 0;

                List<IMessageProcessor> localListeners;
                lock (_listenersLock)
                    localListeners = _listeners.ToList();

                foreach (var processor in localListeners)
                {
                    foreach (var listener in processor.MessageMatcher.GetHandlerLinks(listenId))
                    {
                        processed = true;
                        _logger.ProcessorMatched(SocketId, listener.ToString(), listenId);

                        // 4. Determine the type to deserialize to for this processor
                        var messageType = listener.GetDeserializationType(accessor);
                        if (messageType == null)
                        {
                            _logger.ReceivedMessageNotRecognized(SocketId, processor.Id);
                            continue;
                        }

                        if (processor is Subscription subscriptionProcessor && subscriptionProcessor.Status == SubscriptionStatus.Subscribing)
                        {
                            // If this message is for this listener then it is automatically confirmed, even if the subscription is not (yet) confirmed
                            subscriptionProcessor.Status = SubscriptionStatus.Subscribed;
                            if (subscriptionProcessor.SubscriptionQuery?.TimeoutBehavior == TimeoutBehavior.Succeed)
                                // If this subscription has a query waiting for a timeout (success if there is no error response)
                                // then time it out now as the data is being received, so we assume it's successful
                                subscriptionProcessor.SubscriptionQuery.Timeout();
                        }

                        // 5. Deserialize the message
                        _deserializationCache.TryGetValue(messageType, out var deserialized);

                        if (deserialized == null)
                        {
                            var desResult = processor.Deserialize(accessor, messageType);
                            if (!desResult)
                            {
                                _logger.FailedToDeserializeMessage(SocketId, desResult.Error?.ToString(), desResult.Error?.Exception);
                                continue;
                            }

                            deserialized = desResult.Data;
                            _deserializationCache.Add(messageType, deserialized);
                        }

                        // 6. Pass the message to the handler
                        try
                        {
                            var innerSw = Stopwatch.StartNew();
                            await processor.Handle(this, new DataEvent<object>(deserialized, null, null, originalData, receiveTime, null), listener).ConfigureAwait(false);
                            if (processor is Query query && query.RequiredResponses != 1)
                                _logger.LogDebug($"[Sckt {SocketId}] [Req {query.Id}] responses: {query.CurrentResponses}/{query.RequiredResponses}");
                            totalUserTime += (int)innerSw.ElapsedMilliseconds;
                        }
                        catch (Exception ex)
                        {
                            _logger.UserMessageProcessingFailed(SocketId, ex.Message, ex);
                            if (processor is Subscription subscription)
                                subscription.InvokeExceptionHandler(ex);
                        }

                    }
                }

                if (!processed)
                {
                    if (!ApiClient.UnhandledMessageExpected)
                    {
                        List<string> listenerIds;
                        lock (_listenersLock)
                            listenerIds = _listeners.Select(l => l.MessageMatcher.ToString()).ToList();

                        _logger.ReceivedMessageNotMatchedToAnyListener(SocketId, listenId, string.Join(",", listenerIds));
                        UnhandledMessage?.Invoke(accessor);
                    }

                    return;
                }

                _logger.MessageProcessed(SocketId, sw.ElapsedMilliseconds, sw.ElapsedMilliseconds - totalUserTime);
            }
            finally
            {
                _deserializationCache.Clear();
                accessor.Clear();
            }
        }

        /// <summary>
        /// Connect the websocket
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult> ConnectAsync(CancellationToken ct) => await _socket.ConnectAsync(ct).ConfigureAwait(false);

        /// <summary>
        /// Retrieve the underlying socket
        /// </summary>
        /// <returns></returns>
        public IWebsocket GetSocket() => _socket;

        /// <summary>
        /// Trigger a reconnect of the socket connection
        /// </summary>
        /// <returns></returns>
        public async Task TriggerReconnectAsync() => await _socket.ReconnectAsync().ConfigureAwait(false);

        /// <summary>
        /// Update the proxy setting and reconnect
        /// </summary>
        /// <param name="proxy">New proxy setting</param>
        public async Task UpdateProxy(ApiProxy? proxy)
        {
            _socket.UpdateProxy(proxy);
            await TriggerReconnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            if (Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            if (ApiClient.socketConnections.ContainsKey(SocketId))
                ApiClient.socketConnections.TryRemove(SocketId, out _);

            lock (_listenersLock)
            {
                foreach (var subscription in _listeners.OfType<Subscription>())
                {
                    if (subscription.CancellationTokenRegistration.HasValue)
                        subscription.CancellationTokenRegistration.Value.Dispose();
                }
            }

            await _socket.CloseAsync().ConfigureAwait(false);
            _socket.Dispose();
        }

        /// <summary>
        /// Close a subscription on this connection. If all subscriptions on this connection are closed the connection gets closed as well
        /// </summary>
        /// <param name="subscription">Subscription to close</param>
        /// <returns></returns>
        public async Task CloseAsync(Subscription subscription)
        {
            // If we are resubscribing this subscription at this moment we'll want to wait for a bit until it is finished to avoid concurrency issues
            while (subscription.Status == SubscriptionStatus.Subscribing)
                await Task.Delay(50).ConfigureAwait(false);

            subscription.Status = SubscriptionStatus.Closing;

            if (Status == SocketStatus.Closing || Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
            {
                subscription.Status = SubscriptionStatus.Closed;
                return;
            }

            _logger.ClosingSubscription(SocketId, subscription.Id);
            if (subscription.CancellationTokenRegistration.HasValue)
                subscription.CancellationTokenRegistration.Value.Dispose();

            bool anyDuplicateSubscription;
            lock (_listenersLock)
                anyDuplicateSubscription = _listeners.OfType<Subscription>().Any(x => x != subscription && x.MessageMatcher.HandlerLinks.All(l => subscription.MessageMatcher.ContainsCheck(l)));

            bool shouldCloseConnection;
            lock (_listenersLock)
                shouldCloseConnection = _listeners.OfType<Subscription>().All(r => !r.UserSubscription || r.Status == SubscriptionStatus.Closing || r.Status == SubscriptionStatus.Closed) && !DedicatedRequestConnection.IsDedicatedRequestConnection;

            if (!anyDuplicateSubscription)
            {
                bool needUnsub;
                lock (_listenersLock)
                    needUnsub = _listeners.Contains(subscription) && !shouldCloseConnection;

                if (needUnsub && _socket.IsOpen)
                    await UnsubscribeAsync(subscription).ConfigureAwait(false);
            }
            else
            {
                _logger.NotUnsubscribingSubscriptionBecauseDuplicateRunning(SocketId);
            }

            if (Status == SocketStatus.Closing)
            {
                subscription.Status = SubscriptionStatus.Closed;
                _logger.AlreadyClosing(SocketId);
                return;
            }

            if (shouldCloseConnection)
            {
                Status = SocketStatus.Closing;
                subscription.IsClosingConnection = true;
                _logger.ClosingNoMoreSubscriptions(SocketId);
                await CloseAsync().ConfigureAwait(false);
            }

            lock (_listenersLock)
                _listeners.Remove(subscription);

            subscription.Status = SubscriptionStatus.Closed;
        }

        /// <summary>
        /// Dispose the connection
        /// </summary>
        public void Dispose()
        {
            Status = SocketStatus.Disposed;
            periodicEvent?.Set();
            periodicEvent?.Dispose();
            _socket.Dispose();
        }

        /// <summary>
        /// Whether or not a new subscription can be added to this connection
        /// </summary>
        /// <returns></returns>
        public bool CanAddSubscription() => Status == SocketStatus.None || Status == SocketStatus.Connected;

        /// <summary>
        /// Add a subscription to this connection
        /// </summary>
        /// <param name="subscription"></param>
        public bool AddSubscription(Subscription subscription)
        {
            if (Status != SocketStatus.None && Status != SocketStatus.Connected)
                return false;

            lock (_listenersLock)
                _listeners.Add(subscription);

            if (subscription.UserSubscription)
                _logger.AddingNewSubscription(SocketId, subscription.Id, UserSubscriptionCount);
            return true;
        }

        /// <summary>
        /// Get a subscription on this connection by id
        /// </summary>
        /// <param name="id"></param>
        public Subscription? GetSubscription(int id)
        {
            lock (_listenersLock)
                return _listeners.OfType<Subscription>().SingleOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Get the state of the connection
        /// </summary>
        /// <returns></returns>
        public SocketConnectionState GetState(bool includeSubDetails)
        {
            return new SocketConnectionState(
                SocketId,
                ConnectionUri.AbsoluteUri,
                UserSubscriptionCount,
                Status,
                Authenticated,
                IncomingKbps,
                PendingQueries: _listeners.OfType<Query>().Count(x => !x.Completed),
                includeSubDetails ? Subscriptions.Select(sub => sub.GetState()).ToList() : new List<Subscription.SubscriptionState>()
            );
        }

        /// <summary>
        /// Send a query request and wait for an answer
        /// </summary>
        /// <param name="query">Query to send</param>
        /// <param name="continueEvent">Wait event for when the socket message handler can continue</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        public virtual async Task<CallResult> SendAndWaitQueryAsync(Query query, AsyncResetEvent? continueEvent = null, CancellationToken ct = default)
        {
            await SendAndWaitIntAsync(query, continueEvent, ct).ConfigureAwait(false);
            return query.Result ?? new CallResult(new TimeoutError());
        }

        /// <summary>
        /// Send a query request and wait for an answer
        /// </summary>
        /// <typeparam name="THandlerResponse">Expected result type</typeparam>
        /// <param name="query">Query to send</param>
        /// <param name="continueEvent">Wait event for when the socket message handler can continue</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        public virtual async Task<CallResult<THandlerResponse>> SendAndWaitQueryAsync<THandlerResponse>(Query<THandlerResponse> query, AsyncResetEvent? continueEvent = null, CancellationToken ct = default)
        {
            await SendAndWaitIntAsync(query, continueEvent, ct).ConfigureAwait(false);
            return query.TypedResult ?? new CallResult<THandlerResponse>(new TimeoutError());
        }

        private async Task SendAndWaitIntAsync(Query query, AsyncResetEvent? continueEvent, CancellationToken ct = default)
        {
            lock (_listenersLock)
                _listeners.Add(query);

            query.ContinueAwaiter = continueEvent;
            var sendResult = await SendAsync(query.Id, query.Request, query.Weight).ConfigureAwait(false);
            if (!sendResult)
            {
                query.Fail(sendResult.Error!);
                lock (_listenersLock)
                    _listeners.Remove(query);
                return;
            }

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (!_socket.IsOpen)
                    {
                        query.Fail(new WebError("Socket not open"));
                        return;
                    }

                    if (query.Completed)
                        return;

                    await query.WaitAsync(TimeSpan.FromMilliseconds(500), ct).ConfigureAwait(false);

                    if (query.Completed)
                        return;
                }

                if (ct.IsCancellationRequested)
                {
                    query.Fail(new CancellationRequestedError());
                    return;
                }
            }
            finally
            {
                lock (_listenersLock)
                    _listeners.Remove(query);
            }
        }

        /// <summary>
        /// Send data over the websocket connection
        /// </summary>
        /// <typeparam name="T">The type of the object to send</typeparam>
        /// <param name="requestId">The request id</param>
        /// <param name="obj">The object to send</param>
        /// <param name="weight">The weight of the message</param>
        public virtual ValueTask<CallResult> SendAsync<T>(int requestId, T obj, int weight)
        {
            if (_serializer is IByteMessageSerializer byteSerializer)
            {
                return SendBytesAsync(requestId, byteSerializer.Serialize(obj), weight);
            }
            else if (_serializer is IStringMessageSerializer stringSerializer)
            {
                if (obj is string str)
                    return SendStringAsync(requestId, str, weight);

                str = stringSerializer.Serialize(obj);
                return SendAsync(requestId, str, weight);
            }

            throw new Exception("Unknown serializer when sending message");
        }

        /// <summary>
        /// Send byte data over the websocket connection
        /// </summary>
        /// <param name="data">The data to send</param>
        /// <param name="weight">The weight of the message</param>
        /// <param name="requestId">The id of the request</param>
        public virtual async ValueTask<CallResult> SendBytesAsync(int requestId, byte[] data, int weight)
        {
            if (ApiClient.MessageSendSizeLimit != null && data.Length > ApiClient.MessageSendSizeLimit.Value)
            {
                var info = $"Message to send exceeds the max server message size ({data.Length} vs {ApiClient.MessageSendSizeLimit.Value} bytes). Split the request into batches to keep below this limit";
                _logger.LogWarning("[Sckt {SocketId}] [Req {RequestId}] {Info}", SocketId, requestId, info);
                return new CallResult(new InvalidOperationError(info));
            }

            if (!_socket.IsOpen)
            {
                _logger.LogWarning("[Sckt {SocketId}] [Req {RequestId}] failed to send, socket no longer open", SocketId, requestId);
                return new CallResult(new WebError("Failed to send message, socket no longer open"));
            }

            _logger.SendingByteData(SocketId, requestId, data.Length);
            try
            {
                if (!_socket.Send(requestId, data, weight))
                    return new CallResult(new WebError("Failed to send message, connection not open"));

                return CallResult.SuccessResult;
            }
            catch (Exception ex)
            {
                return new CallResult(new WebError("Failed to send message: " + ex.Message, exception: ex));
            }
        }

        /// <summary>
        /// Send string data over the websocket connection
        /// </summary>
        /// <param name="data">The data to send</param>
        /// <param name="weight">The weight of the message</param>
        /// <param name="requestId">The id of the request</param>
        public virtual async ValueTask<CallResult> SendStringAsync(int requestId, string data, int weight)
        {
            if (ApiClient.MessageSendSizeLimit != null && data.Length > ApiClient.MessageSendSizeLimit.Value)
            {
                var info = $"Message to send exceeds the max server message size ({data.Length} vs {ApiClient.MessageSendSizeLimit.Value} bytes). Split the request into batches to keep below this limit";
                _logger.LogWarning("[Sckt {SocketId}] [Req {RequestId}] {Info}", SocketId, requestId, info);
                return new CallResult(new InvalidOperationError(info));
            }

            if (!_socket.IsOpen)
            {
                _logger.LogWarning("[Sckt {SocketId}] [Req {RequestId}] failed to send, socket no longer open", SocketId, requestId);
                return new CallResult(new WebError("Failed to send message, socket no longer open"));
            }

            _logger.SendingData(SocketId, requestId, data);
            try
            {
                if (!_socket.Send(requestId, data, weight))
                    return new CallResult(new WebError("Failed to send message, connection not open"));

                return CallResult.SuccessResult;
            }
            catch (Exception ex)
            {
                return new CallResult(new WebError("Failed to send message: " + ex.Message, exception: ex));
            }
        }

        private async Task<CallResult> ProcessReconnectAsync()
        {
            if (!_socket.IsOpen)
                return new CallResult(new WebError("Socket not connected"));

            if (!DedicatedRequestConnection.IsDedicatedRequestConnection)
            {
                bool anySubscriptions;
                lock (_listenersLock)
                    anySubscriptions = _listeners.OfType<Subscription>().Any(s => s.UserSubscription);
                if (!anySubscriptions)
                {
                    // No need to resubscribe anything
                    _logger.NothingToResubscribeCloseConnection(SocketId);
                    _ = _socket.CloseAsync();
                    return CallResult.SuccessResult;
                }
            }

            bool anyAuthenticated;
            lock (_listenersLock)
            {
                anyAuthenticated = _listeners.OfType<Subscription>().Any(s => s.Authenticated)
                    || (DedicatedRequestConnection.IsDedicatedRequestConnection && DedicatedRequestConnection.Authenticated);
            }

            if (anyAuthenticated)
            {
                // If we reconnected a authenticated connection we need to re-authenticate
                var authResult = await ApiClient.AuthenticateSocketAsync(this).ConfigureAwait(false);
                if (!authResult)
                {
                    _logger.FailedAuthenticationDisconnectAndRecoonect(SocketId);
                    return authResult;
                }

                Authenticated = true;
                _logger.AuthenticationSucceeded(SocketId);
            }

            // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
            int batch = 0;
            int batchSize = ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket;
            while (true)
            {
                if (!_socket.IsOpen)
                    return new CallResult(new WebError("Socket not connected"));

                List<Subscription> subList;
                lock (_listenersLock)
                    subList = _listeners.OfType<Subscription>().Where(x => x.Active).Skip(batch * batchSize).Take(batchSize).ToList();

                if (subList.Count == 0)
                    break;

                var taskList = new List<Task<CallResult>>();
                foreach (var subscription in subList)
                {
                    subscription.ConnectionInvocations = 0;
                    if (!subscription.Active)
                        // Can be closed during resubscribing
                        continue;

                    subscription.Status = SubscriptionStatus.Subscribing;
                    var result = await ApiClient.RevitalizeRequestAsync(subscription).ConfigureAwait(false);
                    if (!result)
                    {
                        _logger.FailedRequestRevitalization(SocketId, result.Error?.ToString());
                        subscription.Status = SubscriptionStatus.Pending;
                        return result;
                    }

                    var subQuery = subscription.CreateSubscriptionQuery(this);
                    if (subQuery == null)
                    {
                        subscription.Status = SubscriptionStatus.Subscribed;
                        continue;
                    }

                    var waitEvent = new AsyncResetEvent(false);
                    taskList.Add(SendAndWaitQueryAsync(subQuery, waitEvent).ContinueWith((r) =>
                    {
                        subscription.Status = r.Result.Success ? SubscriptionStatus.Subscribed : SubscriptionStatus.Pending;
                        subscription.HandleSubQueryResponse(subQuery.Response!);
                        waitEvent.Set();
                        return r.Result;
                    }));
                }

                await Task.WhenAll(taskList).ConfigureAwait(false);
                if (taskList.Any(t => !t.Result.Success))
                    return taskList.First(t => !t.Result.Success).Result;

                batch++;
            }

            if (!_socket.IsOpen)
                return new CallResult(new WebError("Socket not connected"));

            _logger.AllSubscriptionResubscribed(SocketId);
            return CallResult.SuccessResult;
        }

        internal async Task UnsubscribeAsync(Subscription subscription)
        {
            var unsubscribeRequest = subscription.CreateUnsubscriptionQuery(this);
            if (unsubscribeRequest == null)
                return;

            await SendAndWaitQueryAsync(unsubscribeRequest).ConfigureAwait(false);
            _logger.SubscriptionUnsubscribed(SocketId, subscription.Id);
        }

        internal async Task<CallResult> ResubscribeAsync(Subscription subscription)
        {
            if (!_socket.IsOpen)
                return new CallResult(new WebError("Socket is not connected"));

            var subQuery = subscription.CreateSubscriptionQuery(this);
            if (subQuery == null)
                return CallResult.SuccessResult;

            var result = await SendAndWaitQueryAsync(subQuery).ConfigureAwait(false);
            subscription.HandleSubQueryResponse(subQuery.Response!);
            return result;
        }

        /// <summary>
        /// Periodically sends data over a socket connection
        /// </summary>
        /// <param name="identifier">Identifier for the periodic send</param>
        /// <param name="interval">How often</param>
        /// <param name="queryDelegate">Method returning the query to send</param>
        /// <param name="callback">The callback for processing the response</param>
        public virtual void QueryPeriodic(string identifier, TimeSpan interval, Func<SocketConnection, Query> queryDelegate, Action<SocketConnection, CallResult>? callback)
        {
            if (queryDelegate == null)
                throw new ArgumentNullException(nameof(queryDelegate));

            periodicEvent = new AsyncResetEvent();
            periodicTask = Task.Run(async () =>
            {
                while (Status != SocketStatus.Disposed
                    && Status != SocketStatus.Closed
                    && Status != SocketStatus.Closing)
                {
                    await periodicEvent.WaitAsync(interval).ConfigureAwait(false);
                    if (Status == SocketStatus.Disposed
                    || Status == SocketStatus.Closed
                    || Status == SocketStatus.Closing)
                    {
                        break;
                    }

                    if (!Connected)
                        continue;

                    var query = queryDelegate(this);
                    if (query == null)
                        continue;

                    _logger.SendingPeriodic(SocketId, identifier);

                    try
                    {
                        var result = await SendAndWaitQueryAsync(query).ConfigureAwait(false);
                        callback?.Invoke(this, result);
                    }
                    catch (Exception ex)
                    {
                        _logger.PeriodicSendFailed(SocketId, identifier, ex.Message, ex);
                    }
                }
            });
        }

    }
}

