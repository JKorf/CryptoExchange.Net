using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.OrderBook.V2
{
    internal class SnapshotRegistration
    {
        public int Depth { get; set; }
        public Action<BookSnapshot> Handler { get; set; } = default!;
    }

    /// <summary>
    /// Base for order book implementations
    /// </summary>
    public class ClientOrderBook : Interfaces.IClientOrderBook, IAsyncDisposable
    {
#if NET9_0_OR_GREATER
        private readonly Lock _bookLock = new Lock();
#else
        private readonly object _bookLock = new object();
#endif

        private BookStatus _status;
        private SyncStatus _syncStatus;
        private UpdateSubscription? _subscription;

        private bool _attachedSubscriptionEvents;
        
        private Task? _processTask;
        private CancellationTokenSource? _cts;

        private bool _snapshotApplied;
        private readonly AsyncResetEvent _queueEvent;
        private readonly AsyncResetEvent _updateBufferedEvent;

        private readonly ConcurrentQueue<object> _processQueue;
        private bool _validateChecksum;
        private bool _firstUpdateAfterSnapshotDone;
        private bool _diposed;
        /// <summary>
        /// Sequence number result
        /// </summary>
        public enum SequenceNumberResult
        {
            /// <summary>
            /// The update was skipped
            /// </summary>
            Skip,
            /// <summary>
            /// Update applied
            /// </summary>
            Ok,
            /// <summary>
            /// Update out of sync
            /// </summary>
            OutOfSync
        }

        private readonly object _changedHandlersLock = new();
        private readonly SortedList<int, List<SnapshotRegistration>> _changedHandlers = new();

        /// <summary>
        /// A buffer to store messages received before the initial book snapshot is processed. These messages
        /// will be processed after the book snapshot is set. Any messages in this buffer with sequence numbers lower
        /// than the snapshot sequence number will be discarded
        /// </summary>
        protected readonly List<OrderBookUpdate> _processBuffer;

        /// <summary>
        /// The ask list, should only be accessed using the bookLock
        /// </summary>
        protected SortedList<decimal, decimal> _asks;

        /// <summary>
        /// The bid list, should only be accessed using the bookLock
        /// </summary>
        protected SortedList<decimal, decimal> _bids;

        /// <summary>
        /// The log
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        /// Whether to process updates
        /// </summary>
        protected bool AllowProcessing => 
            (Status == BookStatus.Starting || Status == BookStatus.Running)
            && _cts?.IsCancellationRequested == false;

        /// <inheritdoc/>
        public string Exchange { get; }

        /// <inheritdoc/>
        public BookStatus Status 
        {
            get => _status;
            set
            {
                if (value == _status)
                    return;

                var old = _status;
                _status = value;
                //_logger.OrderBookStatusChanged(_api, _symbol, old, value);

                try
                {
                    OnStatusChange?.Invoke(_status);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Exception in OnStatusChange event");
                }
            }
        }

        /// <inheritdoc/>
        public SyncStatus SyncStatus
        {
            get => _syncStatus;
            set
            {
                if (value == _syncStatus)
                    return;

                lock (_bookLock)
                {
                    var old = _syncStatus;
                    _syncStatus = value;
                }

                //_logger.OrderBookStatusChanged(_api, _symbol, old, value);
                try
                {
                    OnSyncStatusChange?.Invoke(_syncStatus);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in OnSyncStatusChange event");
                }
            }
        }

        /// <inheritdoc/>
        public long LastSequenceNumber { get; private set; }

        /// <inheritdoc/>
        public SharedSymbol Symbol { get; }

        /// <inheritdoc/>
        public event Action<BookStatus>? OnStatusChange;
        /// <inheritdoc/>
        public event Action<SyncStatus>? OnSyncStatusChange;

        /// <inheritdoc/>
        public DateTime UpdateTime { get; private set; }

        /// <inheritdoc/>
        public DateTime? UpdateServerTime { get; private set; }

        /// <inheritdoc/>
        public DateTime? UpdateLocalTime { get; set; }

        /// <inheritdoc/>
        public TimeSpan? DataAge => DateTime.UtcNow - UpdateLocalTime;

        /// <inheritdoc/>
        public (decimal, decimal) BestAsk
        {
            get
            {
                lock (_bookLock)
                    return _asks.Count > 0 ? (_asks.Keys[0], _asks.Values[0]) : (0, 0);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<decimal, decimal> Asks
        {
            get
            {
                lock (_bookLock)
                {
                    return CopyLevels(_asks, _asks.Count);
                }
            }
        }

        /// <inheritdoc/>
        public (decimal, decimal) BestBid
        {
            get
            {
                lock (_bookLock)
                    return _bids.Count > 0 ? (_bids.Keys[0], _bids.Values[0]) : (0, 0);
            }
        }
        /// <inheritdoc/>
        public IReadOnlyDictionary<decimal, decimal> Bids 
        {
            get
            {
                lock (_bookLock)
                {
                    return CopyLevels(_bids, _bids.Count);
                }
            }
        }

        /// <inheritdoc/>
        public BookSnapshot Book
        {
            get
            {
                lock (_bookLock)
                {
                    return new BookSnapshot(LastSequenceNumber, Asks, Bids);
                }
            }
        }

        /// <inheritdoc/>
        public BookSnapshot GetLevels(int levels)
        {
            lock (_bookLock)
            {
                return new BookSnapshot(
                    LastSequenceNumber,
                    CopyLevels(_asks, levels),
                    CopyLevels(_bids, levels));
            }
        }

        private readonly ISubscribeOrderBookSocket? _snapshotApi;
        private readonly ISubscribeIncrementalOrderBookSocket? _incrementalApi;
        private readonly IGetOrderBookRest? _restSnapshotApi;

        private BookConfig _config;
        private Task? _recoverTask;
        private Task<CallResult>? _syncTask;
        private Task? _shutDownTask;
        private readonly object _shutDownLock = new object();
        private int _version;
        private ExchangeParameters? _exchangeParameters;
        private CancellationTokenRegistration? _ctRegistration;
        private string _api;
        private string _symbol;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="config">The book configuration</param>
        /// <param name="snapshotApi">The socket snapshot API</param>
        /// <param name="incrementalApi">The socket incremental API</param>
        /// <param name="restSnapshotApi">The REST snapshot API</param>
        /// <param name="exchangeParameters">Additional exchange parameters</param>
        public ClientOrderBook(
            ILoggerFactory? logger,
            SharedSymbol symbol,
            BookConfig config,
            ISubscribeOrderBookSocket? snapshotApi = null, 
            ISubscribeIncrementalOrderBookSocket? incrementalApi = null,
            IGetOrderBookRest? restSnapshotApi = null,
            ExchangeParameters? exchangeParameters = null)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (snapshotApi == null && incrementalApi == null)
                throw new ArgumentNullException("No API available");

            if (incrementalApi != null && incrementalApi.SubscribeIncrementalOrderBookOptions.UpdateType == SharedOrderBookSubscriptionType.OnlyIncremental && restSnapshotApi == null)
            {
                // If only incremental updates are available, a REST snapshot API is required to get the initial book snapshot
                // If that is not provided try use the snapshot socket API if available, otherwise throw an exception
                if (snapshotApi == null)
                    throw new ArgumentNullException("No REST API available for incremental only subscription");

                _snapshotApi = snapshotApi;
            }
            else
            {
                _snapshotApi = snapshotApi;
                _incrementalApi = incrementalApi;
                _restSnapshotApi = restSnapshotApi;
            }

            Exchange = _incrementalApi?.Exchange ?? _snapshotApi!.Exchange;

            _config = config;

            _exchangeParameters = exchangeParameters;
            _processBuffer = new List<OrderBookUpdate>();
            _processQueue = new ConcurrentQueue<object>();
            _queueEvent = new AsyncResetEvent(false, true);
            _updateBufferedEvent = new AsyncResetEvent(false, false);

            Symbol = symbol;
            _symbol = symbol.BaseAsset + "/" + symbol.QuoteAsset;
            _api = symbol.TradingMode.ToString();

            Status = BookStatus.Stopped;
            SyncStatus = SyncStatus.Idle;

            _asks = new SortedList<decimal, decimal>();
            _bids = new SortedList<decimal, decimal>(new DescComparer<decimal>());

            _logger = logger?.CreateLogger(Exchange) ?? NullLoggerFactory.Instance.CreateLogger(Exchange);
        }

        /// <summary>
        /// Initialize the order book using the provided options
        /// </summary>
        /// <param name="options">The options</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected void Initialize(OrderBookOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _validateChecksum = options.ChecksumValidationEnabled;
        }

        /// <inheritdoc />
        public async Task<CallResult> StartAsync(CancellationToken? ct = null)
        {
            if (_diposed)
                throw new InvalidOperationException($"Book has been disposed");

            if (Status != BookStatus.Stopped)
                throw new InvalidOperationException($"Can't start book unless state is {BookStatus.Stopped}. Current state: {Status}");

            _logger.OrderBookStarting(_api, _symbol);
            Status = BookStatus.Starting;

            _ctRegistration = ct?.Register(() =>
            {
                _ = StopAsync();
            });

            var cts = ct == null ? new CancellationTokenSource() : CancellationTokenSource.CreateLinkedTokenSource(ct.Value);
            _cts = cts;

            Reset();

            _shutDownTask = null;
            _syncTask = null;
            _recoverTask = null;
            _processTask = ProcessQueue();

            var subscription = await DoSubscribeAsync(cts.Token).ConfigureAwait(false);
            if (!subscription.Success)
            {
                await StopAsync().ConfigureAwait(false);
                return CallResult.Fail(subscription.Error!);
            }

            _subscription = subscription.Data;
            _subscription.SubscriptionStatusChanged += SubscriptionStatusChanged;
            _subscription.ConnectionClosed += HandleConnectionClosed;
            _attachedSubscriptionEvents = true;

            var syncResult = await DoSyncAsync(cts.Token).ConfigureAwait(false);
            if (!syncResult.Success)
            {
                await StopAsync().ConfigureAwait(false);
                return CallResult.Fail(syncResult.Error!);
            }

            if (cts.IsCancellationRequested)
            {
                await StopAsync().ConfigureAwait(false);
                return CallResult.Fail(new CancellationRequestedError());
            }

            Status = BookStatus.Running;
            return CallResult.Ok();
        }

        private void SubscriptionStatusChanged(SubscriptionStatus subscriptionStatus)
        {
            if (subscriptionStatus == SubscriptionStatus.Pending
                || subscriptionStatus == SubscriptionStatus.Subscribing)
            {
                // Subscription is reconnecting/resubscribing
                lock (_bookLock) 
                {
                    if (SyncStatus != SyncStatus.Idle)
                        SyncStatus = SyncStatus.OutOfSync;
                    Reset();
                }
            }

            if (subscriptionStatus == SubscriptionStatus.Subscribed)
            {
                _ = DoSyncAsync(_cts?.Token ?? default);
            }    
        }

        private void HandleConnectionClosed()
        {
            _logger.OrderBookDisconnected(_api, _symbol);
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            _logger.OrderBookStopping(_api, _symbol);
            
            lock (_shutDownLock)
            {
                if (_shutDownTask == null)
                {
                    _shutDownTask = ShutdownAsync();
                }
            }

            await _shutDownTask.ConfigureAwait(false);
            _shutDownTask = null;
            _logger.OrderBookStopped(_api, _symbol);
        }

        /// <summary>
        /// Shutdown procedure
        /// </summary>
        /// <returns></returns>
        protected async Task ShutdownAsync()
        {
            await Task.Yield();
            Status = BookStatus.Stopping;
            if (_subscription != null)
            {
                if (_attachedSubscriptionEvents)
                {
                    _subscription.ConnectionClosed -= HandleConnectionClosed;
                    _subscription.SubscriptionStatusChanged -= SubscriptionStatusChanged;
                    _attachedSubscriptionEvents = false;
                }
            }
            
            _ctRegistration?.Dispose();
            _ctRegistration = null;
    
            if (_cts?.IsCancellationRequested == false)
                _cts.Cancel();

            SyncStatus = SyncStatus.Idle;
            if (_processTask != null)
                await _processTask.ConfigureAwait(false);
            if (_syncTask != null)
                await _syncTask.ConfigureAwait(false);
            if (_recoverTask != null)
                await _recoverTask.ConfigureAwait(false);

            if (_subscription != null
                && _subscription.SubscriptionStatus != SubscriptionStatus.Closing
                && _subscription.SubscriptionStatus != SubscriptionStatus.Closed)
            {
                await _subscription.CloseAsync().ConfigureAwait(false);
            }

            if (_subscription != null)
            {
                while (_subscription.SubscriptionStatus != SubscriptionStatus.Closed)
                    await Task.Delay(10).ConfigureAwait(false);
            }

            _subscription = null;
            _cts?.Dispose();
            _cts = null;

            Status = BookStatus.Stopped;
        }

        /// <summary>
        /// Subscribe to websocket updates
        /// </summary>
        protected virtual async Task<WebSocketResult<UpdateSubscription>> DoSubscribeAsync(CancellationToken ct)
        {
            if (_incrementalApi != null)
            {
                return await _incrementalApi.SubscribeToIncrementalOrderBookUpdatesAsync(
                    new SubscribeOrderBookRequest(Symbol, exchangeParameters: _exchangeParameters), HandleUpdate, ct: ct).ConfigureAwait(false);
            }

            return await _snapshotApi!.SubscribeToOrderBookUpdatesAsync(
                    new SubscribeOrderBookRequest(
                        Symbol,
                        _config.Levels,
                        exchangeParameters: _exchangeParameters),
                    HandleUpdate,
                    ct: ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronize the book
        /// </summary>
        protected virtual async Task<CallResult> DoSyncAsync(CancellationToken ct)
        {
            if (_subscription == null)
                throw new InvalidOperationException("Subscription not set");

            if (_syncTask != null && !_syncTask.IsCompleted)
                await _syncTask.ConfigureAwait(false);

            _syncTask = Task.Run(async() =>
            {
                if (SyncStatus == SyncStatus.Synced)
                    return CallResult.Ok();

                SyncStatus = SyncStatus.Syncing;

                if (_incrementalApi != null)
                {
                    if (_incrementalApi.SubscribeIncrementalOrderBookOptions.UpdateType == SharedOrderBookSubscriptionType.OnlyIncremental)
                    {
                        // Only incremental, request REST snapshot
                        // Wait up to 250ms until the first update has been received
                        await WaitUntilFirstUpdateBufferedAsync(_config.MinFirstBufferedUpdateWait, _config.MaxFirstBufferedUpdateWait, ct).ConfigureAwait(false);

                        var snapshot = await _restSnapshotApi!.GetOrderBookAsync(
                            new GetOrderBookRequest(
                                Symbol,
                                _config.Levels,
                                exchangeParameters: _exchangeParameters), 
                            ct: ct).ConfigureAwait(false);

                        if (!snapshot.Success)
                        {
                            _logger.Log(LogLevel.Debug, $"{_api} order book {_symbol} failed to retrieve initial order book");
                            return CallResult.Fail(snapshot.Error!);
                        }

                        _processQueue.Enqueue(
                            new OrderBookSnapshot
                            {
                                SequenceNumber = snapshot.Data.SequenceNumber ?? DateTime.UtcNow.Ticks,
                                Asks = snapshot.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                                Bids = snapshot.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity)
                            });

                        _queueEvent.Set();
                        return await WaitForSyncedBook(_config.SyncTimeout, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        // Snapshot + incremental, wait for snapshot to be received
                        return await WaitForSyncedBook(_config.SyncTimeout, ct).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Snapshot socket API
                    return await WaitForSyncedBook(_config.SyncTimeout, ct).ConfigureAwait(false);
                }
            });

            return await _syncTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Handle a snapshot orderbook update
        /// </summary>
        protected virtual void HandleUpdate(DataEvent<SharedOrderBook> data)
        {
            var sequenceNumber = data.Data.SequenceNumber ?? data.SequenceNumber ?? data.DataTime?.Ticks ?? data.ReceiveTime.Ticks;
            _processQueue.Enqueue(
                    new OrderBookSnapshot
                    {
                        LocalDataTime = data.DataTimeLocal,
                        ServerDataTime = data.DataTime,
                        SequenceNumber = sequenceNumber,
                        Asks = data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                        Bids = data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity)
                    });
            _queueEvent.Set();
        }

        /// <summary>
        /// Handle an incremental orderbook update
        /// </summary>
        protected virtual void HandleUpdate(DataEvent<SharedIncrementalOrderBook> data)
        {
            var sequenceNumber = data.Data.SequenceNumber ?? data.SequenceNumber ?? data.DataTime?.Ticks ?? data.ReceiveTime.Ticks;
            if (data.UpdateType == SocketUpdateType.Snapshot)
            {
                _processQueue.Enqueue(
                    new OrderBookSnapshot
                    {
                        LocalDataTime = data.DataTimeLocal,
                        ServerDataTime = data.DataTime,
                        SequenceNumber = sequenceNumber,
                        Asks = data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                        Bids = data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity)
                    });
                _queueEvent.Set();
                return;
            }

            if (data.Data.StartSequenceNumber == null)
            {
                _processQueue.Enqueue(
                    new OrderBookUpdate
                    {
                        LocalDataTime = data.DataTimeLocal,
                        ServerDataTime = data.DataTime,
                        StartSequenceNumber = sequenceNumber,
                        EndSequenceNumber = sequenceNumber,
                        Asks = data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                        Bids = data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity)
                    });
                _queueEvent.Set();
                return;
            }
            else
            {
                _processQueue.Enqueue(
                    new OrderBookUpdate
                    {
                        LocalDataTime = data.DataTimeLocal,
                        ServerDataTime = data.DataTime,
                        StartSequenceNumber = data.Data.StartSequenceNumber!.Value,
                        EndSequenceNumber = sequenceNumber,
                        Asks = data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                        Bids = data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity)
                    });
                _queueEvent.Set();
                return;
            }
        }

        /// <summary>
        /// Implementation for validating a checksum value with the current order book. If checksum validation fails (returns false)
        /// the order book will be resynchronized
        /// </summary>
        protected virtual bool DoChecksum(int checksum) => true;
        
        /// <summary>
        /// Add a checksum value to the process queue
        /// </summary>
        /// <param name="checksum">The checksum value</param>
        /// <param name="sequenceNumber">The sequence number of the message if it's a separate message with separate number</param>
        protected void AddChecksum(int checksum, long? sequenceNumber = null)
        {
            if (!AllowProcessing)
                throw new InvalidOperationException("Trying to add checksum while book is not working");

            _processQueue.Enqueue(new OrderBookChecksum() { Checksum = checksum, SequenceNumber = sequenceNumber });
            _queueEvent.Set();
        }

        private async Task ProcessQueue()
        {
            while (AllowProcessing)
            {
                try { await _queueEvent.WaitAsync(ct: _cts!.Token).ConfigureAwait(false); } catch (OperationCanceledException) { }

                while (AllowProcessing)
                {
                    int dataVersion;
                    object? item;
                    lock (_bookLock)
                    {
                        if (!_processQueue.TryDequeue(out item))
                            break;

                        dataVersion = _version;
                    }

                    List<(Action<BookSnapshot>[], BookSnapshot)>? updateHandlers = null;
                    lock (_bookLock)
                    {
                        if (!AllowProcessing)
                            break;

                        if (dataVersion != _version || SyncStatus == SyncStatus.OutOfSync)
                            continue;

                        if (item is OrderBookSnapshot snapshot)
                            updateHandlers = ProcessSnapshot(snapshot);
                        else if (item is OrderBookUpdate update)
                            updateHandlers = ProcessUpdate(update);
                        else if (item is OrderBookChecksum checksum)
                            ProcessChecksum(checksum);
                    }

                    if (updateHandlers?.Count > 0)
                    {
                        foreach (var handler in updateHandlers)
                        {
                            foreach (var h in handler.Item1)
                            {
                                try
                                {
                                    h(handler.Item2);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Log(LogLevel.Error, ex, $"Error while executing order book changed handlers: {ex}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<(Action<BookSnapshot>[], BookSnapshot)>? ProcessSnapshot(OrderBookSnapshot item)
        {
            _asks.Clear();
            foreach (var ask in item.Asks)
                _asks.Add(ask.Key, ask.Value);
            _bids.Clear();
            foreach (var bid in item.Bids)
                _bids.Add(bid.Key, bid.Value);

            if (item.SequenceNumber != null)
                LastSequenceNumber = item.SequenceNumber.Value;

            UpdateTime = DateTime.UtcNow;
            UpdateServerTime = item.ServerDataTime;
            UpdateLocalTime = item.LocalDataTime;
            _snapshotApplied = true;

            _logger.OrderBookDataSet(_api, _symbol, _bids.Count, _asks.Count, item.SequenceNumber);
            var appliedBufferResult = ApplyBufferedUpdates();
            if (appliedBufferResult != null)
            {
                if (appliedBufferResult == SequenceNumberResult.OutOfSync)
                {
                    SyncStatus = SyncStatus.OutOfSync;
                    StartRecovery();
                    return null;
                }

                if (_asks.Count > 0 && _bids.Count > 0 && _asks.Keys[0] < _bids.Keys[0])
                {
                    _logger.OrderBookOutOfSyncDetected(_api, _symbol, _asks.Keys[0], _bids.Keys[0]);
                    SyncStatus = SyncStatus.OutOfSync;
                    StartRecovery();
                    return null;
                }
            }            

            SyncStatus = SyncStatus.Synced;

            if (_changedHandlers.Count > 0)
                return CaptureChangedNotifications(1);

            return null;            
        }

        /// <summary>
        /// Check and empty the process buffer; see what entries to update the book with
        /// </summary>
        protected SequenceNumberResult? ApplyBufferedUpdates()
        {
            if (_processBuffer.Count == 0)
                return null;

            var result = SequenceNumberResult.Ok;
            _logger.OrderBookProcessingBufferedUpdates(_api, _symbol, _processBuffer.Count);

            foreach (var bufferEntry in _processBuffer)
            {
                if (!AllowProcessing || SyncStatus == SyncStatus.OutOfSync)
                    break;

                if (ProcessUpdate(bufferEntry.StartSequenceNumber, bufferEntry.EndSequenceNumber, bufferEntry.Bids, bufferEntry.Asks, true, out _) == SequenceNumberResult.OutOfSync)
                {
                    result = SequenceNumberResult.OutOfSync;
                    break;
                }
            }

            _processBuffer.Clear();
            return result;
        }

        private List<(Action<BookSnapshot>[], BookSnapshot)>? ProcessUpdate(OrderBookUpdate item)
        {
            if (!_snapshotApplied)
            {
                _processBuffer.Add(item);
                _updateBufferedEvent.Set();

                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.OrderBookUpdateBuffered(_api, _symbol, item.StartSequenceNumber, item.EndSequenceNumber, item.Asks.Count, item.Bids.Count);
            }
            else
            {
                if (ProcessUpdate(item.StartSequenceNumber, item.EndSequenceNumber, item.Bids, item.Asks, false, out var changedFromEntryOn) == SequenceNumberResult.OutOfSync)
                {
                    SyncStatus = SyncStatus.OutOfSync;
                    StartRecovery();
                    return null;
                }

                if (_asks.Count > 0 && _bids.Count > 0 && _asks.Keys[0] < _bids.Keys[0])
                {
                    _logger.OrderBookOutOfSyncDetected(_api, _symbol, _asks.Keys[0], _bids.Keys[0]);
                    SyncStatus = SyncStatus.OutOfSync;
                    StartRecovery();
                    return null;
                }

                SyncStatus = SyncStatus.Synced;
                UpdateServerTime = item.ServerDataTime;
                UpdateLocalTime = item.LocalDataTime;

                if (changedFromEntryOn != -1
                    && _changedHandlers.Count > 0)
                {
                    return CaptureChangedNotifications(changedFromEntryOn);
                }
            }

            return null;
        }

        private SequenceNumberResult ProcessUpdate(
            long updateSequenceNumberStart,
            long updateSequenceNumberEnd,
            IDictionary<decimal, decimal> bids,
            IDictionary<decimal, decimal> asks,
            bool fromBuffer,
            out int changedFromEntryOn)
        {
            changedFromEntryOn = -1;
            var sequenceResult = fromBuffer ? ValidateBufferSequenceNumber(updateSequenceNumberStart, updateSequenceNumberEnd) : ValidateLiveSequenceNumber(updateSequenceNumberStart);
            if (sequenceResult == SequenceNumberResult.Skip)
            {
                if (updateSequenceNumberStart != updateSequenceNumberEnd)
                    _logger.OrderBookUpdateSkipped(_api, _symbol, updateSequenceNumberStart, updateSequenceNumberEnd, LastSequenceNumber);
                else
                    _logger.OrderBookUpdateSkipped(_api, _symbol, updateSequenceNumberStart, LastSequenceNumber);

                return sequenceResult;
            }

            if (sequenceResult == SequenceNumberResult.OutOfSync)
            {
                _logger.OrderBookOutOfSync(_api, _symbol, LastSequenceNumber + 1, updateSequenceNumberStart);
                return sequenceResult;
            }

            var minUpdatedBidIndex = int.MaxValue;
            var minUpdatedAskIndex = int.MaxValue;
            foreach (var entry in bids)
            {
                UpdateValue(OrderBookEntryType.Bid, entry, out var updatedBidIndex);
                if (updatedBidIndex != null && updatedBidIndex < minUpdatedBidIndex)
                    minUpdatedBidIndex = updatedBidIndex.Value;
            }

            foreach (var entry in asks)
            {
                UpdateValue(OrderBookEntryType.Ask, entry, out var updatedAskIndex);
                if (updatedAskIndex != null && updatedAskIndex < minUpdatedAskIndex)
                    minUpdatedAskIndex = updatedAskIndex.Value;
            }

            if (minUpdatedBidIndex != int.MaxValue || minUpdatedAskIndex != int.MaxValue)
                changedFromEntryOn = Math.Min(minUpdatedBidIndex, minUpdatedAskIndex) + 1; // Index to top

            if (_config.Levels.HasValue && _config.StrictLevels)
            {
                while (_bids.Count > _config.Levels.Value)
                    _bids.RemoveAt(_bids.Count - 1);

                while (_asks.Count > _config.Levels.Value)
                    _asks.RemoveAt(_asks.Count - 1);
            }

            _firstUpdateAfterSnapshotDone = true;
            LastSequenceNumber = updateSequenceNumberEnd;

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                if (updateSequenceNumberStart != updateSequenceNumberEnd)
                    _logger.OrderBookProcessedMessage(_api, _symbol, updateSequenceNumberStart, updateSequenceNumberEnd);
                else
                    _logger.OrderBookProcessedMessage(_api, _symbol, updateSequenceNumberStart);
            }

            return sequenceResult;
        }

        /// <summary>
        /// Update order book with an entry
        /// </summary>
        /// <param name="type">Type of entry</param>
        /// <param name="entry">The entry</param>
        /// <param name="entryIndex">The index of the changed entry</param>
        protected virtual bool UpdateValue(OrderBookEntryType type, KeyValuePair<decimal, decimal> entry, out int? entryIndex)
        {
            UpdateTime = DateTime.UtcNow;
            entryIndex = null;
            var listToChange = type == OrderBookEntryType.Ask ? _asks : _bids;
            if (entry.Value == 0)
            {
                entryIndex = listToChange.IndexOfKey(entry.Key);
                if (entryIndex == -1)
                {
                    entryIndex = null;
                    return true;
                }

                listToChange.RemoveAt(entryIndex.Value);
            }
            else
            {
                entryIndex = listToChange.IndexOfKey(entry.Key);
                if (entryIndex == -1)
                {
                    listToChange.Add(entry.Key, entry.Value);
                    entryIndex = listToChange.IndexOfKey(entry.Key);
                }
                else
                {                    
#if NET8_0_OR_GREATER
                    listToChange.SetValueAtIndex(entryIndex.Value, entry.Value);
#else
                    listToChange[entry.Key] = entry.Value;
#endif
                }
            }

            return true;
        }

        private void ProcessChecksum(OrderBookChecksum ci)
        {
            if (!_validateChecksum)
                return;

            bool checksumResult = false;
            try
            {
                checksumResult = DoChecksum(ci.Checksum);
            }
            catch (Exception)
            {
                // If the status is not synced it can be expected a checksum is failing
                if (SyncStatus == SyncStatus.Synced)
                    throw;
            }

            if (ci.SequenceNumber != null)
                LastSequenceNumber = ci.SequenceNumber.Value;

            if (!checksumResult)
            {
                _logger.OrderBookOutOfSyncChecksum(_api, _symbol);
                SyncStatus = SyncStatus.OutOfSync;
                StartRecovery();
            }
        }

        private void Reset()
        {
            _processBuffer.Clear();
            _queueEvent.Set();
            // Clear queue
            while (_processQueue.TryDequeue(out _)) { }

            LastSequenceNumber = 0;
            _version++;

            _snapshotApplied = false;

            _updateBufferedEvent.Reset();

            _asks.Clear();
            _bids.Clear();

            _firstUpdateAfterSnapshotDone = false;            
        }

        private void StartRecovery()
        {
            if (_recoverTask != null && !_recoverTask.IsCompleted)
            {
                // Already recovering
                return;
            }

            _recoverTask = Task.Run(async () => 
            {
                if (!AllowProcessing)
                    return;

                var resubResult = await _subscription!.ResubscribeAsync().ConfigureAwait(false);
                if (!resubResult.Success)
                {
                    // Resubscribing failed, reconnect the socket
                    _logger.OrderBookResyncFailed(_api, _symbol);
                    await _subscription!.ReconnectAsync().ConfigureAwait(false);
                }
            });
        }

        /// <summary>
        /// IAsyncDisposable implementation for the order book
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (_diposed)
            {
                if (_shutDownTask != null)
                    await _shutDownTask.ConfigureAwait(false);

                return;
            }

            lock (_shutDownLock)
            {
                if (_shutDownTask == null)
                {
                    _shutDownTask = ShutdownAsync();
                }
            }

            _diposed = true;

            await _shutDownTask.ConfigureAwait(false);
            _shutDownTask = null;

            lock (_changedHandlersLock)
                _changedHandlers.Clear();
            OnStatusChange = null;
            OnSyncStatusChange = null;
        }

        /// <summary>
        /// Wait until the order book has been synced
        /// </summary>
        /// <param name="timeout">Max wait time</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected async Task<CallResult> WaitForSyncedBook(TimeSpan timeout, CancellationToken ct)
        {
            var startTime = DateTime.UtcNow;
            while (SyncStatus == SyncStatus.Syncing)
            {
                if (ct.IsCancellationRequested)
                    return CallResult.Fail(new CancellationRequestedError());

                if (startTime + timeout < DateTime.UtcNow)
                    return CallResult.Fail(new ServerError(new ErrorInfo(ErrorType.OrderBookTimeout, "Timeout while waiting for book to sync")));

                try { await Task.Delay(10, ct).ConfigureAwait(false); } catch { }
            }

            if (SyncStatus == SyncStatus.OutOfSync)
                return CallResult.Fail(new ServerError(new ErrorInfo(ErrorType.Unknown, "Order book is out of sync")));

            return CallResult.Ok();        }

        /// <summary>
        /// Wait until an update has been buffered
        /// </summary>
        /// <param name="minWait">Min wait time</param>
        /// <param name="maxWait">Max wait time</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected async Task<CallResult> WaitUntilFirstUpdateBufferedAsync(TimeSpan? minWait, TimeSpan maxWait, CancellationToken ct)
        {
            var startWait = DateTime.UtcNow;
            if (!await _updateBufferedEvent.WaitAsync(maxWait, ct).ConfigureAwait(false))
                return CallResult.Fail(new ServerError(new ErrorInfo(ErrorType.OrderBookTimeout, "Timeout while waiting for update to be buffered")));

            var dif = DateTime.UtcNow - startWait;
            if (minWait != null && dif < minWait)
                await Task.Delay(minWait.Value - dif).ConfigureAwait(false);

            return CallResult.Ok();
        }

        private SequenceNumberResult ValidateBufferSequenceNumber(long startSequenceNumber, long endSequenceNumber)
        {
            if (endSequenceNumber <= LastSequenceNumber)
                // Buffered update is from before the snapshot, ignore
                return SequenceNumberResult.Skip;

            if (_config.SequencesAreConsecutive && startSequenceNumber != LastSequenceNumber + 1)
            {
                if (_firstUpdateAfterSnapshotDone || !_config.SkipSequenceCheckFirstUpdateAfterSnapshotSet)
                    // Buffered update is not the next sequence number when it was expected to be
                    return SequenceNumberResult.OutOfSync;
            }

            // Buffered sequence number is larger than the last sequence number
            return SequenceNumberResult.Ok;
        }

        private SequenceNumberResult ValidateLiveSequenceNumber(long sequenceNumber)
        {
            if (sequenceNumber < LastSequenceNumber
                && (_firstUpdateAfterSnapshotDone || !_config.SkipSequenceCheckFirstUpdateAfterSnapshotSet))
            {
                // Update is somehow from before the current state
                return SequenceNumberResult.OutOfSync;
            }

            if (_config.SequencesAreConsecutive
                && LastSequenceNumber != 0
                && sequenceNumber != LastSequenceNumber + 1)
            {
                if (_firstUpdateAfterSnapshotDone || !_config.SkipSequenceCheckFirstUpdateAfterSnapshotSet)
                    return SequenceNumberResult.OutOfSync;                
            }

            return SequenceNumberResult.Ok;
        }

        private List<(Action<BookSnapshot>[] Handlers, BookSnapshot Snapshot)>? CaptureChangedNotifications(int firstChangedLevel)
        {
            List<(int Depth, Action<BookSnapshot>[] Handlers)>? groups = null;

            lock (_changedHandlersLock)
            {
                foreach (var group in _changedHandlers)
                {
                    if (group.Key < firstChangedLevel || group.Value.Count == 0)
                        continue;

                    groups ??= new();
                    groups.Add((group.Key, group.Value.Select(r => r.Handler).ToArray()));
                }
            }

            if (groups is null)
                return null;

            var notifications =
                new List<(Action<BookSnapshot>[] Handlers, BookSnapshot Snapshot)>(groups.Count);

            foreach (var (depth, handlers) in groups)
            {
                var snapshot = new BookSnapshot(LastSequenceNumber, CopyLevels(_asks, depth), CopyLevels(_bids, depth));
                notifications.Add((handlers, snapshot));
            }

            return notifications;
        }

        private static IReadOnlyDictionary<decimal, decimal> CopyLevels(
            SortedList<decimal, decimal> levels, int depth)
        {
            var size = Math.Min(levels.Count, depth);
            var copy = new Dictionary<decimal, decimal>(size);
            for (var i = 0; i < size; i++)
                copy.Add(levels.Keys[i], levels.Values[i]);

            return copy;
        }

        /// <inheritdoc />
        public void RegisterChangedHandler(Action<BookSnapshot> handler)
            => RegisterChangedHandler(int.MaxValue, handler);

        /// <inheritdoc />
        public void RegisterChangedHandler(int top, Action<BookSnapshot> handler)
        {
            if (top <= 0)
                throw new ArgumentException("Top must be greater than 0", nameof(top));

            lock (_changedHandlersLock)
            {
                if (!_changedHandlers.TryGetValue(top, out var handlerList))
                    _changedHandlers[top] = handlerList = new List<SnapshotRegistration>();

                handlerList.Add(new SnapshotRegistration { Depth = top, Handler = handler });
            }
        }

        /// <inheritdoc />
        public void UnregisterChangedHandler(Action<BookSnapshot> handler)
        {
            lock (_changedHandlersLock)
            {
                foreach (var handlerList in _changedHandlers.Values)
                    handlerList.RemoveAll(h => h.Handler == handler);
            }
        }

        /// <inheritdoc />
        public void UnregisterChangedHandlers()
        {
            lock (_changedHandlersLock)
            {
                _changedHandlers.Clear();
            }
        }


        /// <inheritdoc/>
        public CallResult<decimal> CalculateAverageFillPrice(decimal quantity, OrderBookEntryType type)
        {
            if (SyncStatus != SyncStatus.Synced)
                return CallResult<decimal>.Fail(new InvalidOperationError($"{nameof(CalculateAverageFillPrice)} is not available when book is not in Synced state"));

            var totalCost = 0m;
            var totalAmount = 0m;
            var amountLeft = quantity;
            lock (_bookLock)
            {
                var list = type == OrderBookEntryType.Ask ? _asks : _bids;

                var step = 0;
                while (amountLeft > 0)
                {
                    if (step == list.Count)
                        return CallResult<decimal>.Fail(new InvalidOperationError("Quantity is larger than order in the order book"));

                    var quanty = list.Values[step];
                    var stepAmount = Math.Min(quanty, amountLeft);
                    totalCost += stepAmount * list.Keys[step];
                    totalAmount += stepAmount;
                    amountLeft -= stepAmount;
                    step++;
                }
            }

            if (totalAmount == 0)
                return CallResult<decimal>.Fail(new InvalidOperationError("No amount available in the order book"));

            return CallResult<decimal>.Ok(Math.Round(totalCost / totalAmount, 8));
        }

        /// <inheritdoc/>
        public CallResult<decimal> CalculateTradableAmount(decimal quoteQuantity, OrderBookEntryType type)
        {
            if (SyncStatus != SyncStatus.Synced)
                return CallResult<decimal>.Fail(new InvalidOperationError($"{nameof(CalculateTradableAmount)} is not available when book is not in Synced state"));

            var quoteQuantityLeft = quoteQuantity;
            var totalBaseQuantity = 0m;
            lock (_bookLock)
            {
                var list = type == OrderBookEntryType.Ask ? _asks : _bids;

                var step = 0;
                while (quoteQuantityLeft > 0)
                {
                    if (step == list.Count)
                        return CallResult<decimal>.Fail(new InvalidOperationError("Quantity is larger than order in the order book"));

                    var quantity = list.Values[step];
                    var price = list.Keys[step];
                    var stepAmount = Math.Min(quantity * price, quoteQuantityLeft);
                    quoteQuantityLeft -= stepAmount;
                    totalBaseQuantity += stepAmount / price;
                    step++;
                }
            }

            return CallResult<decimal>.Ok(Math.Round(totalBaseQuantity, 8));
        }


        /// <summary>
        /// String representation of the top 3 entries
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(3);
        }

        /// <summary>
        /// String representation of the top x entries
        /// </summary>
        /// <returns></returns>
        public string ToString(int numberOfEntries)
        {
            var stringBuilder = new StringBuilder();
            var book = Book;
            stringBuilder.AppendLine($"{Exchange} - {Symbol}");
            stringBuilder.AppendLine($"Update time local:     {UpdateTime:HH:mm:ss.fff} ({Math.Round((DateTime.UtcNow - UpdateTime).TotalMilliseconds)}ms ago)");
            stringBuilder.AppendLine($"Data timestamp server: {UpdateServerTime:HH:mm:ss.fff}");
            stringBuilder.AppendLine($"Data timestamp local:  {UpdateLocalTime:HH:mm:ss.fff}");
            stringBuilder.AppendLine($"Data age:              {DataAge?.TotalMilliseconds}ms");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"   Ask quantity       Ask price | Bid price       Bid quantity");
            for (var i = 0; i < numberOfEntries; i++)
            {
                var ask = book.Asks.Count > i ? book.Asks.ElementAt(i) : default(KeyValuePair<decimal, decimal>);
                var bid = book.Bids.Count > i ? book.Bids.ElementAt(i) : default(KeyValuePair<decimal, decimal>);
                stringBuilder.AppendLine($"[{ask.Value.ToString(CultureInfo.InvariantCulture),14}] {ask.Key.ToString(CultureInfo.InvariantCulture),14} | {bid.Key.ToString(CultureInfo.InvariantCulture),-14} [{bid.Value.ToString(CultureInfo.InvariantCulture),-14}]");
            }

            return stringBuilder.ToString();
        }

        /// <inheritdoc />
        public Task OutputToConsoleAsync(int numberOfEntries, TimeSpan refreshInterval, CancellationToken ct = default)
        {
            return Task.Run(async () =>
            {
                var referenceTime = DateTime.UtcNow;
                while (!ct.IsCancellationRequested)
                {
                    Console.Clear();
                    Console.WriteLine(ToString(numberOfEntries));
                    var delay = Math.Max(1, (DateTime.UtcNow - referenceTime).TotalMilliseconds % refreshInterval.TotalMilliseconds);
                    try { await Task.Delay(refreshInterval.Add(TimeSpan.FromMilliseconds(-delay)), ct).ConfigureAwait(false); } catch { }
                }
            });
        }

    }
}
