using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.Objects;
using System.Net.WebSockets;
using CryptoExchange.Net.Objects.Sockets;
using System.Diagnostics;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Logging.Extensions;
using System.Threading;
using System.IO.Pipelines;
using System.Text.Json;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// A single socket connection to the server
    /// </summary>
    public abstract class HighPerfSocketConnection : ISocketConnection
    {
        /// <summary>
        /// Connection closed and no reconnect is happening
        /// </summary>
        public event Action? ConnectionClosed;

        /// <inheritdoc />
        public bool Authenticated { get; set; } = false;

        /// <summary>
        /// The amount of subscriptions on this connection
        /// </summary>
        public int UserSubscriptionCount => _subscriptions.Length;

        /// <summary>
        /// Get a copy of the current message subscriptions
        /// </summary>
        public HighPerfSubscription[] Subscriptions => _subscriptions;

        /// <summary>
        /// If connection is made
        /// </summary>
        public bool Connected => _socket.IsOpen;

        /// <summary>
        /// The unique ID of the socket
        /// </summary>
        public int SocketId => _socket.Id;

        /// <summary>
        /// The connection uri
        /// </summary>
        public Uri ConnectionUri => _socket.Uri;

        /// <summary>
        /// The API client the connection is for
        /// </summary>
        public SocketApiClient ApiClient { get; set; }

        /// <summary>
        /// Tag for identification
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Additional properties for this connection
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

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

        private readonly ILogger _logger;
        private SocketStatus _status;
        private readonly IMessageSerializer _serializer;
        protected readonly JsonSerializerOptions _serializerOptions;
        protected readonly Pipe _pipe;
        private Task? _processTask;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        protected abstract HighPerfSubscription[] _subscriptions { get; }

        public abstract Type UpdateType { get; }

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
        private readonly IHighPerfWebsocket _socket;

        /// <summary>
        /// New socket connection
        /// </summary>
        public HighPerfSocketConnection(ILogger logger, IWebsocketFactory socketFactory, WebSocketParameters parameters, SocketApiClient apiClient, JsonSerializerOptions serializerOptions, string tag)
        {
            _logger = logger;
            _pipe = new Pipe(new PipeOptions
            {
                //ReaderScheduler
            });
            _serializerOptions = serializerOptions;
            ApiClient = apiClient;
            Tag = tag;
            Properties = new Dictionary<string, object>();

            _socket = socketFactory.CreateHighPerfWebsocket(logger, parameters, _pipe.Writer);
            _logger.SocketCreatedForAddress(_socket.Id, parameters.Uri.ToString());

            _socket.OnOpen += HandleOpenAsync;
            _socket.OnClose += HandleCloseAsync;

            _socket.OnError += HandleErrorAsync;

            _serializer = apiClient.CreateSerializer();
        }

        protected abstract Task ProcessAsync(CancellationToken ct);

        /// <summary>
        /// Handler for a socket opening
        /// </summary>
        protected virtual Task HandleOpenAsync()
        {
            Status = SocketStatus.Connected;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for a socket closing without reconnect
        /// </summary>
        protected virtual async Task HandleCloseAsync()
        {
            Status = SocketStatus.Closed;
            _cts.Cancel();

            if (ApiClient.highPerfSocketConnections.ContainsKey(SocketId))
                ApiClient.highPerfSocketConnections.TryRemove(SocketId, out _);

            await _processTask!.ConfigureAwait(false);

            _ = Task.Run(() => ConnectionClosed?.Invoke());
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
        /// Connect the websocket
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult> ConnectAsync(CancellationToken ct)
        {
            var result = await _socket.ConnectAsync(ct).ConfigureAwait(false);
            if (result.Success)
                _processTask = ProcessAsync(_cts.Token);

            return result;
        }

        /// <summary>
        /// Retrieve the underlying socket
        /// </summary>
        /// <returns></returns>
        public IHighPerfWebsocket GetSocket() => _socket;

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

            foreach (var subscription in _subscriptions)
            {
                if (subscription.CancellationTokenRegistration.HasValue)
                    subscription.CancellationTokenRegistration.Value.Dispose();
            }

            await _socket.CloseAsync().ConfigureAwait(false);
            _socket.Dispose();
        }

        /// <summary>
        /// Close a subscription on this connection. If all subscriptions on this connection are closed the connection gets closed as well
        /// </summary>
        /// <param name="subscription">Subscription to close</param>
        /// <returns></returns>
        public async Task CloseAsync(HighPerfSubscription subscription)
        {
            if (Status == SocketStatus.Closing || Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            _logger.ClosingSubscription(SocketId, subscription.Id);
            if (subscription.CancellationTokenRegistration.HasValue)
                subscription.CancellationTokenRegistration.Value.Dispose();

            var anyOtherSubscriptions = _subscriptions.Any(x => x != subscription);

            if (anyOtherSubscriptions)
                await UnsubscribeAsync(subscription).ConfigureAwait(false);

            if (Status == SocketStatus.Closing)
            {
                _logger.AlreadyClosing(SocketId);
                return;
            }

            if (!anyOtherSubscriptions)
            {
                Status = SocketStatus.Closing;
                _logger.ClosingNoMoreSubscriptions(SocketId);
                await CloseAsync().ConfigureAwait(false);
            }

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
        /// Send data over the websocket connection
        /// </summary>
        /// <typeparam name="T">The type of the object to send</typeparam>
        /// <param name="obj">The object to send</param>
        public virtual ValueTask<CallResult> SendAsync<T>(T obj)
        {
            if (_serializer is IByteMessageSerializer byteSerializer)
            {
                return SendBytesAsync(byteSerializer.Serialize(obj));
            }
            else if (_serializer is IStringMessageSerializer stringSerializer)
            {
                if (obj is string str)
                    return SendStringAsync(str);

                str = stringSerializer.Serialize(obj);
                return SendStringAsync(str);
            }

            throw new Exception("Unknown serializer when sending message");
        }

        /// <summary>
        /// Send byte data over the websocket connection
        /// </summary>
        /// <param name="data">The data to send</param>
        public virtual async ValueTask<CallResult> SendBytesAsync(byte[] data)
        {
            if (ApiClient.MessageSendSizeLimit != null && data.Length > ApiClient.MessageSendSizeLimit.Value)
            {
                var info = $"Message to send exceeds the max server message size ({data.Length} vs {ApiClient.MessageSendSizeLimit.Value} bytes). Split the request into batches to keep below this limit";
                _logger.LogWarning("[Sckt {SocketId}] {Info}", SocketId, info);
                return new CallResult(new InvalidOperationError(info));
            }

            if (!_socket.IsOpen)
            {
                _logger.LogWarning("[Sckt {SocketId}] Request failed to send, socket no longer open", SocketId);
                return new CallResult(new WebError("Failed to send message, socket no longer open"));
            }

            _logger.SendingByteData(SocketId, 0, data.Length);
            try
            {
                if (!await _socket.SendAsync(data).ConfigureAwait(false))
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
        public virtual async ValueTask<CallResult> SendStringAsync(string data)
        {
            if (ApiClient.MessageSendSizeLimit != null && data.Length > ApiClient.MessageSendSizeLimit.Value)
            {
                var info = $"Message to send exceeds the max server message size ({data.Length} vs {ApiClient.MessageSendSizeLimit.Value} bytes). Split the request into batches to keep below this limit";
                _logger.LogWarning("[Sckt {SocketId}] {Info}", SocketId, info);
                return new CallResult(new InvalidOperationError(info));
            }

            if (!_socket.IsOpen)
            {
                _logger.LogWarning("[Sckt {SocketId}] Request failed to send, socket no longer open", SocketId);
                return new CallResult(new WebError("Failed to send message, socket no longer open"));
            }

            _logger.SendingData(SocketId, 0, data);
            try
            {
                if (!await _socket.SendAsync(data).ConfigureAwait(false))
                    return new CallResult(new WebError("Failed to send message, connection not open"));

                return CallResult.SuccessResult;
            }
            catch (Exception ex)
            {
                return new CallResult(new WebError("Failed to send message: " + ex.Message, exception: ex));
            }
        }

        internal async Task UnsubscribeAsync(HighPerfSubscription subscription)
        {
            var unsubscribeRequest = subscription.CreateUnsubscriptionQuery(this);
            if (unsubscribeRequest == null)
                return;

            await SendAsync(unsubscribeRequest).ConfigureAwait(false);
            _logger.SubscriptionUnsubscribed(SocketId, subscription.Id);
        }

        /// <summary>
        /// Periodically sends data over a socket connection
        /// </summary>
        /// <param name="identifier">Identifier for the periodic send</param>
        /// <param name="interval">How often</param>
        /// <param name="queryDelegate">Method returning the query to send</param>
        public virtual void QueryPeriodic(string identifier, TimeSpan interval, Func<HighPerfSocketConnection, object> queryDelegate)
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
                        var result = await SendAsync(query).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.PeriodicSendFailed(SocketId, identifier, ex.Message, ex);
                    }
                }
            });
        }
    }

    public class HighPerfSocketConnection<T> : HighPerfSocketConnection
    {
        private readonly object _listenersLock = new object();
        private List<HighPerfSubscription<T>> _typedSubscriptions;
        protected override HighPerfSubscription[] _subscriptions
        {
            get
            {
                lock (_listenersLock)
                    return _typedSubscriptions.Select(x => (HighPerfSubscription)x).ToArray();
            }
        }

        public override Type UpdateType => typeof(T);

        public HighPerfSocketConnection(ILogger logger, IWebsocketFactory socketFactory, WebSocketParameters parameters, SocketApiClient apiClient, JsonSerializerOptions serializerOptions, string tag) : base(logger, socketFactory, parameters, apiClient, serializerOptions, tag)
        {
            _typedSubscriptions = new List<HighPerfSubscription<T>>();
        }

        /// <summary>
        /// Add a subscription to this connection
        /// </summary>
        /// <param name="subscription"></param>
        public bool AddSubscription(HighPerfSubscription<T> subscription)
        {
            if (Status != SocketStatus.None && Status != SocketStatus.Connected)
                return false;

            //lock (_listenersLock)
            _typedSubscriptions.Add(subscription);

            //_logger.AddingNewSubscription(SocketId, subscription.Id, UserSubscriptionCount);
            return true;
        }

        public void RemoveSubscription(HighPerfSubscription<T> subscription)
        {
            lock (_listenersLock)
                _typedSubscriptions.Remove(subscription);
        }

        protected override async Task ProcessAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var update in JsonSerializer.DeserializeAsyncEnumerable<T>(_pipe.Reader, true, _serializerOptions, ct).ConfigureAwait(false))
                {
                    var tasks = _typedSubscriptions.Select(sub => sub.HandleAsync(update!));
                    await LibraryHelpers.WhenAll(tasks).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}

