using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
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

        private OrderBookStatus _status;
        private UpdateSubscription? _subscription;
        private bool _attachedSubscriptionEvents;
        
        private Task? _processTask;
        private CancellationTokenSource? _cts;

        private readonly AsyncResetEvent _queueEvent;
        private readonly ConcurrentQueue<object> _processQueue;
        private bool _validateChecksum;
        private bool _firstUpdateAfterSnapshotDone;

        private enum SequenceNumberResult
        {
            Skip,
            Ok,
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
        /// Whether update numbers are consecutive. If set to true and an update comes in which isn't the previous sequences number + 1
        /// the book will resynchronize as it is deemed out of sync
        /// </summary>
        protected bool _sequencesAreConsecutive;

        /// <summary>
        /// Whether the first update message after a snapshot may have overlapping sequence numbers instead of the snapshot sequence number + 1
        /// </summary>
        protected bool _skipSequenceCheckFirstUpdateAfterSnapshotSet;

        /// <summary>
        /// Whether levels should be strictly enforced. For example, when an order book has 25 levels and a new update comes in which pushes
        /// the current level 25 ask out of the top 25, should the level 26 entry be removed from the book or does the server handle this
        /// </summary>
        protected bool _strictLevels;

        /// <summary>
        /// If the initial snapshot of the book has been set
        /// </summary>
        protected bool _bookSet;

        /// <summary>
        /// The amount of levels for this book
        /// </summary>
        protected int? Levels { get; set; } = null;

        protected bool ProcessingStatus => Status == OrderBookStatus.Synced || Status == OrderBookStatus.Syncing;

        /// <inheritdoc/>
        public string Exchange { get; }

        /// <inheritdoc/>
        public OrderBookStatus Status 
        {
            get => _status;
            set
            {
                if (value == _status)
                    return;

                var old = _status;
                _status = value;
                _logger.OrderBookStatusChanged(_api, _symbol, old, value);
                OnStatusChange?.Invoke(old, _status);
            }
        }

        /// <inheritdoc/>
        public long LastSequenceNumber { get; private set; }

        /// <inheritdoc/>
        public SharedSymbol Symbol { get; }

        /// <inheritdoc/>
        public event Action<OrderBookStatus, OrderBookStatus>? OnStatusChange;

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

        private string _api;
        private string _symbol;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="snapshotApi">The socket snapshot API</param>
        /// <param name="incrementalApi">The socket incremental API</param>
        /// <param name="restSnapshotApi">The REST snapshot API</param>
        public ClientOrderBook(
            ILoggerFactory? logger,
            SharedSymbol symbol,
            ISubscribeOrderBookSocket? snapshotApi = null, 
            ISubscribeIncrementalOrderBookSocket? incrementalApi = null,
            IGetOrderBookRest? restSnapshotApi = null)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (snapshotApi == null && incrementalApi == null)
                throw new ArgumentNullException("No API available");

            _snapshotApi = snapshotApi;
            _incrementalApi = incrementalApi;
            _restSnapshotApi = restSnapshotApi;

            Exchange = _incrementalApi?.Exchange ?? _snapshotApi!.Exchange;

            _processBuffer = new List<OrderBookUpdate>();
            _processQueue = new ConcurrentQueue<object>();
            _queueEvent = new AsyncResetEvent(false, true);

            Symbol = symbol;
            _symbol = symbol.BaseAsset + "/" + symbol.QuoteAsset;
            _api = symbol.TradingMode.ToString();

            Status = OrderBookStatus.Disconnected;

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
            if (Status != OrderBookStatus.Disconnected)
                throw new InvalidOperationException($"Can't start book unless state is {OrderBookStatus.Disconnected}. Current state: {Status}");

            _logger.OrderBookStarting(_api, _symbol);   
            _cts = ct == null ? new CancellationTokenSource() : CancellationTokenSource.CreateLinkedTokenSource(ct.Value);

            // Clear any previous messages
            while (_processQueue.TryDequeue(out _)) { }

            _processBuffer.Clear();
            _bookSet = false;
            _firstUpdateAfterSnapshotDone = false;

            Status = OrderBookStatus.Connecting;
            _processTask = ProcessQueue();

            var startResult = await DoStartAsync(_cts.Token).ConfigureAwait(false);
            if (!startResult.Success)
            {
                await StopAsync().ConfigureAwait(false);
                return CallResult.Fail(startResult.Error!);
            }

            if (_cts.IsCancellationRequested)
            {
                await StopAsync().ConfigureAwait(false);
                return CallResult.Fail(new CancellationRequestedError());
            }

            _subscription = startResult.Data;
            _subscription.ConnectionLost += HandleConnectionLost;
            _subscription.ConnectionClosed += HandleConnectionClosed;
            _subscription.ConnectionRestored += HandleConnectionRestored;
            _attachedSubscriptionEvents = true;            

            Status = OrderBookStatus.Synced;
            return CallResult.Ok();
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

        private void HandleConnectionLost() 
        {
            _logger.OrderBookConnectionLost(_api, _symbol);    
            if (Status != OrderBookStatus.Disposed) {
                Status = OrderBookStatus.Reconnecting;
                Reset();
            }
        }

        private void HandleConnectionClosed() {
            _logger.OrderBookDisconnected(_api, _symbol);
            Status = OrderBookStatus.Disconnected;
            _ = StopAsync();
        }

        private async void HandleConnectionRestored(TimeSpan _) {
            await ResyncAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            _logger.OrderBookStopping(_api, _symbol);
            await ShutdownAsync().ConfigureAwait(false);
            Status = OrderBookStatus.Disconnected;
            _logger.OrderBookStopped(_api, _symbol);
        }

        /// <summary>
        /// Shutdown procedure
        /// </summary>
        /// <returns></returns>
        protected async Task ShutdownAsync()
        {
            if (_subscription != null)
            {
                if (_attachedSubscriptionEvents)
                {
                    _subscription.ConnectionLost -= HandleConnectionLost;
                    _subscription.ConnectionClosed -= HandleConnectionClosed;
                    _subscription.ConnectionRestored -= HandleConnectionRestored;
                    _attachedSubscriptionEvents = false;
                }
            }

            if (_cts?.IsCancellationRequested == false)
                _cts.Cancel();

            if (_processTask != null)
                await _processTask.ConfigureAwait(false);

            if (_subscription != null
                && _subscription.SubscriptionStatus != SubscriptionStatus.Closing
                && _subscription.SubscriptionStatus != SubscriptionStatus.Closed)
            {
                await _subscription.CloseAsync().ConfigureAwait(false);
            }
            _subscription = null;
            _cts?.Dispose();
            _cts = null;
        }

        /// <inheritdoc/>
        public CallResult<decimal> CalculateAverageFillPrice(decimal baseQuantity, OrderBookEntryType type)
        {
            if (Status != OrderBookStatus.Synced)
                return CallResult<decimal>.Fail(new InvalidOperationError($"{nameof(CalculateAverageFillPrice)} is not available when book is not in Synced state"));

            var totalCost = 0m;
            var totalAmount = 0m;
            var amountLeft = baseQuantity;
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
            if (Status != OrderBookStatus.Synced)
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
        /// Implementation for starting the order book. Should typically have logic for subscribing to the update stream and retrieving
        /// and setting the initial order book
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<CallResult<UpdateSubscription>> DoStartAsync(CancellationToken ct)
        {
            if (_incrementalApi != null)
            {
                var subResult = await _incrementalApi.SubscribeToIncrementalOrderBookUpdatesAsync(
                    new SubscribeOrderBookRequest(Symbol), HandleUpdate, ct: ct).ConfigureAwait(false);

                if (!subResult.Success)
                    return CallResult<UpdateSubscription>.Fail(subResult.Error!);

                if (ct.IsCancellationRequested)
                    return CallResult<UpdateSubscription>.Fail(new CancellationRequestedError());

                Status = OrderBookStatus.Syncing;

                if (_incrementalApi.SubscribeIncrementalOrderBookOptions.UpdateType == SharedOrderBookSubscriptionType.OnlyIncremental)
                {
                    // Wait up to 250ms until the first update has been received
                    await WaitUntilFirstUpdateBufferedAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(250), ct).ConfigureAwait(false);

                    var snapshot = await _restSnapshotApi!.GetOrderBookAsync(new GetOrderBookRequest(Symbol,
                        _restSnapshotApi.GetOrderBookOptions.MaxLimit ?? _restSnapshotApi.GetOrderBookOptions.SupportedLimits!.Max()), ct: ct).ConfigureAwait(false);

                    if (!snapshot.Success)
                    {
                        _logger.Log(LogLevel.Debug, $"{_api} order book {_symbol} failed to retrieve initial order book");
                        return CallResult<UpdateSubscription>.Fail(snapshot.Error!);
                    }

                    SetSnapshot(
                        snapshot.Data.SequenceNumber,
                        snapshot.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity),
                        snapshot.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity));
                }

                var waitSnapshotResult = await WaitForSetOrderBookAsync(TimeSpan.FromSeconds(10), ct).ConfigureAwait(false);
                if (!waitSnapshotResult.Success)
                {
                    // No snapshot received within 10 seconds
                    return CallResult<UpdateSubscription>.Fail(waitSnapshotResult.Error!);
                }

                return CallResult<UpdateSubscription>.Ok(subResult.Data);
            }

            var snapshotSubResult = await _snapshotApi!.SubscribeToOrderBookUpdatesAsync(
                    new SubscribeOrderBookRequest(
                        Symbol,
                        _snapshotApi.SubscribeOrderBookOptions.SupportedLimits.Max()), HandleUpdate, ct: ct).ConfigureAwait(false);
            if (!snapshotSubResult.Success)
                return CallResult<UpdateSubscription>.Fail(snapshotSubResult.Error!);

            Status = OrderBookStatus.Syncing;

            var result = await WaitForSetOrderBookAsync(TimeSpan.FromSeconds(10), ct).ConfigureAwait(false);
            if (!result.Success)
            {
                // No snapshot received within 10 seconds
                return CallResult<UpdateSubscription>.Fail(result.Error!);
            }

            return CallResult<UpdateSubscription>.Ok(snapshotSubResult.Data);
        }

        /// <summary>
        /// Handle a snapshot orderbook update
        /// </summary>
        protected virtual void HandleUpdate(DataEvent<SharedOrderBook> data)
        {
            if (data.Data.SequenceNumber == null)
                throw new Exception("Sequence number null");

            SetSnapshot(
                data.Data.SequenceNumber!.Value,
                data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity),
                data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                data.DataTime,
                data.DataTimeLocal);
        }

        /// <summary>
        /// Handle an incremental orderbook update
        /// </summary>
        protected virtual void HandleUpdate(DataEvent<SharedIncrementalOrderBook> data)
        {
            if (data.Data.SequenceNumber == null)
                throw new Exception("Sequence number null");

            var sequenceNumber = data.Data.SequenceNumber ?? data.SequenceNumber ?? data.DataTime?.Ticks ?? data.ReceiveTime.Ticks;
            if (data.UpdateType == SocketUpdateType.Snapshot)
            {
                SetSnapshot(
                    sequenceNumber,
                    data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity),
                    data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                    data.DataTime,
                    data.DataTimeLocal);
                return;
            }

            if (data.Data.StartSequenceNumber == null)
            {
                UpdateOrderBook(
                    sequenceNumber,
                    data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity),
                    data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                    data.DataTime,
                    data.DataTimeLocal);
            }
            else
            {
                UpdateOrderBook(
                    data.Data.StartSequenceNumber!.Value,
                    data.Data.SequenceNumber!.Value,
                    data.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity),
                    data.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity),
                    data.DataTime,
                    data.DataTimeLocal);
            }
        }

        /// <summary>
        /// Reset the order book
        /// </summary>
        protected virtual void DoReset() { }

        /// <summary>
        /// Resync the order book
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<CallResult> DoResyncAsync(CancellationToken ct)
        {
            if (_incrementalApi != null)
            {
                // Wait up to 250ms until the first update has been received
                await WaitUntilFirstUpdateBufferedAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(250), ct).ConfigureAwait(false);

                if (_incrementalApi.SubscribeIncrementalOrderBookOptions.UpdateType == SharedOrderBookSubscriptionType.OnlyIncremental)
                {
                    var snapshot = await _restSnapshotApi!.GetOrderBookAsync(new GetOrderBookRequest(Symbol,
                        _restSnapshotApi.GetOrderBookOptions.MaxLimit ?? _restSnapshotApi.GetOrderBookOptions.SupportedLimits!.Max()), ct: ct).ConfigureAwait(false);
                    if (!snapshot.Success)
                        return CallResult.Fail(snapshot.Error!);

                    SetSnapshot(
                        snapshot.Data.SequenceNumber,
                        snapshot.Data.Bids.ToDictionary(x => x.Price, x => x.Quantity),
                        snapshot.Data.Asks.ToDictionary(x => x.Price, x => x.Quantity));
                }

            }

            return await WaitForSetOrderBookAsync(TimeSpan.FromSeconds(10), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Implementation for validating a checksum value with the current order book. If checksum validation fails (returns false)
        /// the order book will be resynchronized
        /// </summary>
        protected virtual bool DoChecksum(int checksum) => true;

        /// <summary>
        /// Set snapshot data for the order book. Typically the snapshot which was requested from the Rest API, or the first snapshot
        /// received from a socket subscription. Will clear any previous data.
        /// </summary>
        /// <param name="orderBookSequenceNumber">The last update sequence number until which the snapshot is in sync</param>
        /// <param name="askList">List of asks</param>
        /// <param name="bidList">List of bids</param>
        /// <param name="serverDataTime">Server data timestamp</param>
        /// <param name="localDataTime">local data timestamp</param>
        protected void SetSnapshot(
            long? orderBookSequenceNumber,
            IDictionary<decimal, decimal> bidList,
            IDictionary<decimal, decimal> askList,
            DateTime? serverDataTime = null,
            DateTime? localDataTime = null)
        {
            if (Status == OrderBookStatus.Disposed || Status == OrderBookStatus.Disconnected)
                throw new InvalidOperationException("Trying to set snapshot while book is not working");

            _processQueue.Enqueue(
                new OrderBookSnapshot
                {
                    LocalDataTime = localDataTime,
                    ServerDataTime = serverDataTime,
                    SequenceNumber = orderBookSequenceNumber,
                    Asks = askList,
                    Bids = bidList
                });
            _queueEvent.Set();
        }

        /// <summary>
        /// Add an update to the process queue. Updates the book by providing changed bids and asks, along with an update number which should be higher than the previous update numbers
        /// </summary>
        /// <param name="sequenceNumber">The sequence number</param>
        /// <param name="bids">List of updated/new bids</param>
        /// <param name="asks">List of updated/new asks</param>
        /// <param name="serverDataTime">Server data timestamp</param>
        /// <param name="localDataTime">local data timestamp</param>
        protected void UpdateOrderBook(
            long sequenceNumber,
            IDictionary<decimal, decimal> bids,
            IDictionary<decimal, decimal> asks,
            DateTime? serverDataTime = null,
            DateTime? localDataTime = null)
        {
            if (Status == OrderBookStatus.Disposed || Status == OrderBookStatus.Disconnected)
                throw new InvalidOperationException("Trying to update order book while book is not working");

            _processQueue.Enqueue(
                new OrderBookUpdate
                { 
                    LocalDataTime = localDataTime,
                    ServerDataTime = serverDataTime,
                    StartSequenceNumber = sequenceNumber, 
                    EndSequenceNumber = sequenceNumber,
                    Asks = asks,
                    Bids = bids 
                });
            _queueEvent.Set();
        }

        /// <summary>
        /// Add an update to the process queue. Updates the book by providing changed bids and asks, along with the first and last sequence number in the update
        /// </summary>
        /// <param name="firstSequenceNumber">The sequence number of the first update</param>
        /// <param name="lastSequenceNumber">The sequence number of the last update</param>
        /// <param name="bids">List of updated/new bids</param>
        /// <param name="asks">List of updated/new asks</param>
        /// <param name="serverDataTime">Server data timestamp</param>
        /// <param name="localDataTime">local data timestamp</param>
        protected void UpdateOrderBook(
            long firstSequenceNumber,
            long lastSequenceNumber,
            IDictionary<decimal, decimal> bids,
            IDictionary<decimal, decimal> asks,
            DateTime? serverDataTime = null,
            DateTime? localDataTime = null)
        {
            if (Status == OrderBookStatus.Disposed || Status == OrderBookStatus.Disconnected)
                throw new InvalidOperationException("Trying to update order book while book is not working");

            _processQueue.Enqueue(
                new OrderBookUpdate
                {
                    LocalDataTime = localDataTime,
                    ServerDataTime = serverDataTime,
                    StartSequenceNumber = firstSequenceNumber,
                    EndSequenceNumber = lastSequenceNumber,
                    Asks = asks,
                    Bids = bids
                });
            _queueEvent.Set();
        }

        /// <summary>
        /// Add a checksum value to the process queue
        /// </summary>
        /// <param name="checksum">The checksum value</param>
        /// <param name="sequenceNumber">The sequence number of the message if it's a separate message with separate number</param>
        protected void AddChecksum(int checksum, long? sequenceNumber = null)
        {
            if (Status == OrderBookStatus.Disposed || Status == OrderBookStatus.Disconnected)
                throw new InvalidOperationException("Trying to add checksum while book is not working");

            _processQueue.Enqueue(new OrderBookChecksum() { Checksum = checksum, SequenceNumber = sequenceNumber });
            _queueEvent.Set();
        }

        /// <summary>
        /// Check and empty the process buffer; see what entries to update the book with
        /// </summary>
        protected void CheckProcessBuffer()
        {
            if (_processBuffer.Count > 0)
                _logger.OrderBookProcessingBufferedUpdates(_api, _symbol, _processBuffer.Count);

            foreach (var bufferEntry in _processBuffer)
            {
                if (_cts.IsCancellationRequested || !ProcessingStatus)
                    break;

                ProcessUpdate(bufferEntry.StartSequenceNumber, bufferEntry.EndSequenceNumber, bufferEntry.Bids, bufferEntry.Asks, true, out _);
                if (_asks.Count > 0 && _bids.Count > 0 && _asks.Keys[0] < _bids.Keys[0])
                {
                    _logger.OrderBookOutOfSyncDetected(_api, _symbol, _asks.Keys[0], _bids.Keys[0]);
                    Resubscribe();
                    return;
                }
            }

            _processBuffer.Clear();
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

        /// <summary>
        /// Wait until the order book snapshot has been set
        /// </summary>
        /// <param name="timeout">Max wait time</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        protected async Task<CallResult> WaitForSetOrderBookAsync(TimeSpan timeout, CancellationToken ct)
        {
            var startWait = DateTime.UtcNow;
            while (!_bookSet && Status == OrderBookStatus.Syncing)
            {
                if(ct.IsCancellationRequested)
                    return CallResult.Fail(new CancellationRequestedError());

                if (DateTime.UtcNow - startWait > timeout)
                    return CallResult.Fail(new ServerError(new ErrorInfo(ErrorType.OrderBookTimeout, "Timeout while waiting for data")));

                try
                {
                    await Task.Delay(50, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                { }
            }

            return CallResult.Ok();
        }

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
            while (_processBuffer.Count == 0)
            {
                if (ct.IsCancellationRequested)
                    return CallResult.Fail(new CancellationRequestedError());

                if (DateTime.UtcNow - startWait > maxWait)
                    return CallResult.Fail(new ServerError(new ErrorInfo(ErrorType.OrderBookTimeout, "Timeout while waiting for data")));

                try
                {
                    await Task.Delay(20, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                { }
            }

            if (minWait != null)
            {
                var dif = DateTime.UtcNow - startWait;
                if (dif < minWait)
                    await Task.Delay(minWait.Value - dif).ConfigureAwait(false);
            }

            return CallResult.Ok();
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
            Status = OrderBookStatus.Disposing;
            await ShutdownAsync().ConfigureAwait(false);
            lock (_changedHandlersLock)
                _changedHandlers.Clear();
            Status = OrderBookStatus.Disposed;
            OnStatusChange = null;
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
            for(var i = 0; i < numberOfEntries; i++)
            {
                var ask = book.Asks.Count > i ? book.Asks.ElementAt(i): default(KeyValuePair<decimal, decimal>);
                var bid = book.Bids.Count > i ? book.Bids.ElementAt(i): default(KeyValuePair<decimal, decimal>);
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

        private void Reset()
        {
            _queueEvent.Set();
            // Clear queue
            while (_processQueue.TryDequeue(out _)) { }

            LastSequenceNumber = 0;
            _processBuffer.Clear();
            _bookSet = false;
            _firstUpdateAfterSnapshotDone = false;

            DoReset();
        }

        private async Task ResyncAsync()
        {
            Status = OrderBookStatus.Syncing;
            var success = false;
            while (!success)
            {
                if (Status != OrderBookStatus.Syncing)
                    return;

                var resyncResult = await DoResyncAsync(_cts!.Token).ConfigureAwait(false);
                success = resyncResult.Success;
            }

            _logger.OrderBookResynced(_api, _symbol);
            Status = OrderBookStatus.Synced;
        }

        private async Task ProcessQueue()
        {
            while (!_cts.IsCancellationRequested)
            {
                try { await _queueEvent.WaitAsync(ct: _cts!.Token).ConfigureAwait(false); } catch (OperationCanceledException) { }

                if (_cts.IsCancellationRequested)
                    break;

                while (_processQueue.TryDequeue(out var item))
                {
                    if (_cts.IsCancellationRequested)
                        break;

                    if (item is OrderBookSnapshot snapshot)
                        ProcessOrderBookSnapshot(snapshot);
                    else if (item is OrderBookUpdate update)
                        ProcessQueueItem(update);
                    else if (item is OrderBookChecksum checksum)
                        ProcessChecksum(checksum);
                }
            }
        }

        private void ProcessOrderBookSnapshot(OrderBookSnapshot item)
        {
            List<(Action<BookSnapshot>[], BookSnapshot)>? updateHandlers = null;
            lock (_bookLock)
            {
                _bookSet = true;

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

                _logger.OrderBookDataSet(_api, _symbol, _bids.Count, _asks.Count, item.SequenceNumber);
                CheckProcessBuffer();

                if (_changedHandlers.Count > 0)
                    updateHandlers = CaptureChangedNotifications(1);
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

        private void ProcessQueueItem(OrderBookUpdate item)
        {
            List<(Action<BookSnapshot>[], BookSnapshot)>? updateHandlers = null;
            lock (_bookLock)
            {
                if (!_bookSet)
                {
                    _processBuffer.Add(item);

                    if (_logger.IsEnabled(LogLevel.Trace))
                        _logger.OrderBookUpdateBuffered(_api, _symbol, item.StartSequenceNumber, item.EndSequenceNumber, item.Asks.Count, item.Bids.Count);
                }
                else
                {
                    CheckProcessBuffer();
                    ProcessUpdate(item.StartSequenceNumber, item.EndSequenceNumber, item.Bids, item.Asks, false, out var changedFromEntryOn);

                    if (_asks.Count > 0 && _bids.Count > 0 && _asks.Keys[0] < _bids.Keys[0])
                    {
                        _logger.OrderBookOutOfSyncDetected(_api, _symbol, _asks.Keys[0], _bids.Keys[0]);
                        Resubscribe();
                        return;
                    }

                    UpdateServerTime = item.ServerDataTime;
                    UpdateLocalTime = item.LocalDataTime;
                    
                    if (changedFromEntryOn != -1
                        && _changedHandlers.Count > 0)
                    {
                        updateHandlers = CaptureChangedNotifications(changedFromEntryOn);
                    }
                }
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

        private void ProcessChecksum(OrderBookChecksum ci)
        {
            lock (_bookLock)
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
                    if (Status == OrderBookStatus.Synced)
                        throw;
                }

                if (ci.SequenceNumber != null)
                    LastSequenceNumber = ci.SequenceNumber.Value;

                if (!checksumResult)
                {
                    _logger.OrderBookOutOfSyncChecksum(_api, _symbol);
                    Resubscribe();
                }
            }
        }

        private void Resubscribe()
        {
            Status = OrderBookStatus.Syncing;

            _ = Task.Run(async () =>
            {
                if(_subscription == null)
                {
                    Status = OrderBookStatus.Disconnected;
                    return;
                }

                await _subscription!.UnsubscribeAsync().ConfigureAwait(false);
                Reset();
                var resubResult = await _subscription!.ResubscribeAsync().ConfigureAwait(false);
                if (!resubResult.Success)
                {
                    // Resubscribing failed, reconnect the socket
                    _logger.OrderBookResyncFailed(_api, _symbol);
                    Status = OrderBookStatus.Reconnecting;
                    _ = _subscription!.ReconnectAsync();
                }
                else
                {
                    await ResyncAsync().ConfigureAwait(false);
                }
            });
        }

        private void ProcessUpdate(
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

                return;
            }

            if (sequenceResult == SequenceNumberResult.OutOfSync)
            {
                _logger.OrderBookOutOfSync(_api, _symbol, LastSequenceNumber + 1, updateSequenceNumberStart);
                Resubscribe();
                return;
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

            if (Levels.HasValue && _strictLevels)
            {
                while (_bids.Count > Levels.Value)                
                    _bids.RemoveAt(_bids.Count - 1);

                while (_asks.Count > Levels.Value)
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
        }

        private SequenceNumberResult ValidateBufferSequenceNumber(long startSequenceNumber, long endSequenceNumber)
        {
            if (endSequenceNumber <= LastSequenceNumber)
                // Buffered update is from before the snapshot, ignore
                return SequenceNumberResult.Skip;

            if (_sequencesAreConsecutive && startSequenceNumber != LastSequenceNumber + 1)
            {
                if (_firstUpdateAfterSnapshotDone || !_skipSequenceCheckFirstUpdateAfterSnapshotSet)
                    // Buffered update is not the next sequence number when it was expected to be
                    return SequenceNumberResult.OutOfSync;
            }

            // Buffered sequence number is larger than the last sequence number
            return SequenceNumberResult.Ok;
        }

        private SequenceNumberResult ValidateLiveSequenceNumber(long sequenceNumber)
        {
            if (sequenceNumber < LastSequenceNumber
                && (_firstUpdateAfterSnapshotDone || !_skipSequenceCheckFirstUpdateAfterSnapshotSet))
            {
                // Update is somehow from before the current state
                return SequenceNumberResult.OutOfSync;
            }

            if (_sequencesAreConsecutive
                && LastSequenceNumber != 0
                && sequenceNumber != LastSequenceNumber + 1)
            {
                if (_firstUpdateAfterSnapshotDone || !_skipSequenceCheckFirstUpdateAfterSnapshotSet)
                    return SequenceNumberResult.OutOfSync;                
            }

            return SequenceNumberResult.Ok;
        }
    }
}
