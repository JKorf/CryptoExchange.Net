using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Trades
{
    /// <inheritdoc />
    public class TradeTracker : ITradeTracker
    {
        private readonly ITradeSocketClient _socketClient;
        private readonly IRecentTradeRestClient? _recentRestClient;
        private readonly ITradeHistoryRestClient? _historyRestClient;
        private readonly string _symbolName;
        private SyncStatus _status;
        private long _snapshotId;
        private bool _startWithSnapshot;

        /// <summary>
        /// The internal data structure
        /// </summary>
        protected readonly List<SharedTrade> _data = new List<SharedTrade>();
        /// <summary>
        /// The pre-snapshot queue buffering updates received before the snapshot is set and which will be applied after the snapshot was set
        /// </summary>
        protected readonly List<SharedTrade> _preSnapshotQueue = new List<SharedTrade>();

        /// <summary>
        /// The last time the window was applied
        /// </summary>
        protected DateTime _lastWindowApplied = DateTime.MinValue;
        /// <summary>
        /// Whether or not the data has changed since last window was applied
        /// </summary>
        protected bool _changed = false;
        /// <summary>
        /// Lock for accessing _data
        /// </summary>
        protected readonly object _lock = new object();
        /// <summary>
        /// Max numer of items tracked
        /// </summary>
        protected readonly int? _limit;
        /// <summary>
        /// Max age of the data
        /// </summary>
        protected readonly TimeSpan? _period;
        /// <summary>
        /// The symbol
        /// </summary>
        protected readonly SharedSymbol _symbol;
        /// <summary>
        /// Whether the snapshot has been set
        /// </summary>
        protected bool _snapshotSet;
        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;
        /// <summary>
        /// Update subscription
        /// </summary>
        protected UpdateSubscription? _updateSubscription;

        /// <summary>
        /// The timestamp of the first item
        /// </summary>
        protected DateTime? _firstTimestamp;

        /// <inheritdoc/>
        public SyncStatus Status
        {
            get => _status;
            set
            {
                if (value == _status)
                    return;

                var old = _status;
                _status = value;
                _logger.TradeTrackerStatusChanged(_symbolName, old, value);
                OnStatusChanged?.Invoke(old, _status);
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    ApplyWindow(true);
                    return _data.Count;
                }
            }
        }

        /// <inheritdoc />
        public DateTime? SyncedFrom
        {
            get
            {
                if (_period == null)
                    return _firstTimestamp;

                var max = DateTime.UtcNow - _period.Value;
                if (_firstTimestamp > max)
                    return _firstTimestamp;

                return max;
            }
        }

        /// <inheritdoc />
        public SharedTrade? Last
        {
            get
            {
                lock (_lock)
                {
                    ApplyWindow(true);
                    return _data.LastOrDefault();
                }
            }
        }

        /// <inheritdoc />
        public event Func<SharedTrade, Task>? OnAdded;
        /// <inheritdoc />
        public event Func<SharedTrade, Task>? OnRemoved;
        /// <inheritdoc />
        public event Func<SyncStatus, SyncStatus, Task>? OnStatusChanged;

        /// <summary>
        /// ctor
        /// </summary>
        public TradeTracker(
            ILogger? logger,
            IRecentTradeRestClient? recentRestClient,
            ITradeHistoryRestClient? historyRestClient,
            ITradeSocketClient socketClient,
            SharedSymbol symbol,
            int? limit = null,
            TimeSpan? period = null)
        {
            _logger = logger ?? new TraceLogger();
            _recentRestClient = recentRestClient;
            _historyRestClient = historyRestClient;
            _socketClient = socketClient;
            _symbol = symbol;
            _symbolName = symbol.BaseAsset + "/" + symbol.QuoteAsset;
            _limit = limit;
            _period = period;
        }

        private TradesStats GetStats(IEnumerable<SharedTrade> trades)
        {
            if (!trades.Any())
                return new TradesStats();

            return new TradesStats
            {
                TradeCount = trades.Count(),
                FirstTradeTime = trades.First().Timestamp,
                LastTradeTime = trades.Last().Timestamp,
                AveragePrice = Math.Round(trades.Select(d => d.Price).DefaultIfEmpty().Average(), 8),
                VolumeWeightedAveragePrice = trades.Any() ? Math.Round(trades.Select(d => d.Price * d.Quantity).DefaultIfEmpty().Sum() / trades.Select(d => d.Quantity).DefaultIfEmpty().Sum(), 8) : null,
                Volume = Math.Round(trades.Sum(d => d.Quantity), 8),
                QuoteVolume = Math.Round(trades.Sum(d => d.Quantity * d.Price), 8),
                BuySellRatio = Math.Round(trades.Where(x => x.Side == SharedOrderSide.Buy).Sum(x => x.Quantity) / trades.Sum(x => x.Quantity), 8)
            };
        }

        /// <inheritdoc />
        public TradesStats GetStats(DateTime? fromTimestamp = null, DateTime? toTimestamp = null)
        {
            var compareTime = SyncedFrom?.AddSeconds(-2);
            var stats = GetStats(GetData(fromTimestamp, toTimestamp));
            stats.Complete = (fromTimestamp == null || fromTimestamp >= compareTime) && (toTimestamp == null || toTimestamp >= compareTime);
            return stats;
        }


        /// <inheritdoc />
        public async Task<CallResult> StartAsync(bool startWithSnapshot = true)
        {
            if (Status != SyncStatus.Disconnected)
                throw new InvalidOperationException($"Can't start syncing unless state is {SyncStatus.Disconnected}. Current state: {Status}");

            _startWithSnapshot = startWithSnapshot;
            Status = SyncStatus.Syncing;
            _logger.TradeTrackerStarting(_symbolName);
            var subResult = await DoStartAsync().ConfigureAwait(false);
            if (!subResult)
            {
                _logger.TradeTrackerStartFailed(_symbolName, subResult.Error!.ToString());
                Status = SyncStatus.Disconnected;
                return subResult;
            }

            _updateSubscription = subResult.Data;
            _updateSubscription.ConnectionLost += HandleConnectionLost;
            _updateSubscription.ConnectionClosed += HandleConnectionClosed;
            _updateSubscription.ConnectionRestored += HandleConnectionRestored;
            SetSyncStatus();
            _logger.TradeTrackerStarted(_symbolName);
            return new CallResult(null);
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            _logger.TradeTrackerStopping(_symbolName);
            Status = SyncStatus.Disconnected;
            await DoStopAsync().ConfigureAwait(false);
            _data.Clear();
            _preSnapshotQueue.Clear();
            _logger.TradeTrackerStopped(_symbolName);
        }

        /// <summary>
        /// The start procedure needed for trade syncing, generally subscribing to an update stream and requesting the snapshot
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<CallResult<UpdateSubscription>> DoStartAsync()
        {
            var subResult = await _socketClient.SubscribeToTradeUpdatesAsync(new SubscribeTradeRequest(_symbol),
                 update =>
                 {
                     AddData(update.Data);
                 }).ConfigureAwait(false);

            if (!subResult)
            {
                Status = SyncStatus.Disconnected;
                return subResult;
            }

            if (!_startWithSnapshot)
                return subResult;

            if (_historyRestClient != null)
            {
                var startTime = _period == null ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.Add(-_period.Value);
                var request = new GetTradeHistoryRequest(_symbol, startTime, DateTime.UtcNow);
                var data = new List<SharedTrade>();
                await foreach(var result in ExchangeHelpers.ExecutePages(_historyRestClient.GetTradeHistoryAsync, request).ConfigureAwait(false))
                {
                    if (!result)
                    {
                        _ = subResult.Data.CloseAsync();
                        Status = SyncStatus.Disconnected;
                        return subResult.AsError<UpdateSubscription>(result.Error!);
                    }

                    if (_limit != null && data.Count > _limit)
                        break;

                    data.AddRange(result.Data);
                }

                SetInitialData(data);
            }
            else if (_recentRestClient != null)
            {
                int? limit = null;
                if (_limit.HasValue)
                    limit = Math.Min(_recentRestClient.GetRecentTradesOptions.MaxLimit, _limit.Value);

                var snapshot = await _recentRestClient.GetRecentTradesAsync(new GetRecentTradesRequest(_symbol, limit)).ConfigureAwait(false);
                if (!snapshot)
                {
                    _ = subResult.Data.CloseAsync();
                    Status = SyncStatus.Disconnected;
                    return subResult.AsError<UpdateSubscription>(snapshot.Error!);
                }

                SetInitialData(snapshot.Data);
            }

            return subResult;
        }

        /// <summary>
        /// The stop procedure needed, generally stopping the update stream
        /// </summary>
        /// <returns></returns>
        protected virtual Task DoStopAsync() => _updateSubscription?.CloseAsync() ?? Task.CompletedTask;

        /// <inheritdoc />
        public IEnumerable<SharedTrade> GetData(DateTime? since = null, DateTime? until = null)
        {
            lock (_lock)
            {
                ApplyWindow(true);

                IEnumerable<SharedTrade> result = _data;
                if (since != null)
                    result = result.Where(d => d.Timestamp >= since);
                if (until != null)
                    result = result.Where(d => d.Timestamp <= until);

                return result.ToList();
            }
        }

        /// <summary>
        /// Set the initial trade data snapshot
        /// </summary>
        /// <param name="data"></param>
        protected void SetInitialData(IEnumerable<SharedTrade> data)
        {
            lock (_lock)
            {
                _data.Clear();

                IEnumerable<SharedTrade> items = data.OrderByDescending(d => d.Timestamp);
                if (_limit != null)
                    items = items.Take(_limit.Value);
                if (_period != null)
                    items = items.Where(e => e.Timestamp >= DateTime.UtcNow.Add(-_period.Value));

                _snapshotId = data.Max(d => d.Timestamp.Ticks);
                foreach (var item in items.OrderBy(d => d.Timestamp))
                    _data.Add(item);

                _snapshotSet = true;
                _changed = true;

                _logger.TradeTrackerInitialDataSet(_symbolName, _data.Count, _snapshotId);

                foreach (var item in _preSnapshotQueue)
                {
                    if (_snapshotId >= item.Timestamp.Ticks)
                    {
                        _logger.TradeTrackerPreSnapshotSkip(_symbolName, item.Timestamp.Ticks);
                        continue;
                    }

                    _logger.TradeTrackerPreSnapshotApplied(_symbolName, item.Timestamp.Ticks);
                    _data.Add(item);
                }

                _firstTimestamp = _data.Min(v => v.Timestamp);

                ApplyWindow(false);
            }
        }

        /// <summary>
        /// Add a trade
        /// </summary>
        /// <param name="item"></param>
        protected void AddData(SharedTrade item) => AddData(new[] { item });

        /// <summary>
        /// Add a list of trades
        /// </summary>
        /// <param name="items"></param>
        protected void AddData(IEnumerable<SharedTrade> items)
        {
            lock (_lock)
            {
                if ((_recentRestClient != null || _historyRestClient != null) && _startWithSnapshot && !_snapshotSet)
                {
                    _preSnapshotQueue.AddRange(items);
                    return;
                }

                foreach (var item in items)
                {
                    _logger.TradeTrackerTradeAdded(_symbolName, item.Timestamp.Ticks);
                    _data.Add(item);
                    OnAdded?.Invoke(item);
                }

                _firstTimestamp = _data.Min(x => x.Timestamp);
                _changed = true;
                SetSyncStatus();
                ApplyWindow(true);
            }
        }

        private void ApplyWindow(bool broadcastEvents)
        {
            if (!_changed && (DateTime.UtcNow - _lastWindowApplied) < TimeSpan.FromSeconds(1))
                return;

            if (_period != null)
            {
                var compareDate = DateTime.UtcNow.Add(-_period.Value);
                for(var i = 0; i < _data.Count; i++)
                {
                    var item = _data[i];
                    if (item.Timestamp >= compareDate)
                        break;

                    _data.Remove(item);
                    if (broadcastEvents)
                        OnRemoved?.Invoke(item);

                    i--;
                }
            }

            if (_limit != null && _data.Count > _limit.Value)
            {
                var toRemove = _data.Count - _limit.Value;
                for (var i = 0; i < toRemove; i++)
                {
                    var item = _data[i];
                    _data.Remove(item);
                    if (broadcastEvents)
                        OnRemoved?.Invoke(item);

                    i--;
                }
            }

            _lastWindowApplied = DateTime.UtcNow;
            _changed = false;
        }


        private void HandleConnectionLost()
        {
            _logger.TradeTrackerConnectionLost(_symbolName);
            if (Status != SyncStatus.Disconnected)
            {
                Status = SyncStatus.Syncing;
                _snapshotSet = false;
                _firstTimestamp = null;
                _preSnapshotQueue.Clear();
            }
        }

        private void HandleConnectionClosed()
        {
            _logger.TradeTrackerConnectionClosed(_symbolName);
            Status = SyncStatus.Disconnected;
            _ = StopAsync();
        }

        private async void HandleConnectionRestored(TimeSpan _)
        {
            Status = SyncStatus.Syncing;
            var success = false;
            while (!success)
            {
                if (Status != SyncStatus.Syncing)
                    return;

                var resyncResult = await DoStartAsync().ConfigureAwait(false);
                success = resyncResult;
            }

            _logger.TradeTrackerConnectionRestored(_symbolName);
            SetSyncStatus();
        }

        private void SetSyncStatus()
        {
            if (Status == SyncStatus.Synced)
                return;

            if (_period != null)
            {
                if (_firstTimestamp <= DateTime.UtcNow - _period.Value)
                    Status = SyncStatus.Synced;
                else
                    Status = SyncStatus.PartiallySynced;
            }

            if (_limit != null)
            {
                if (_data.Count == _limit.Value)
                    Status = SyncStatus.Synced;
                else
                    Status = SyncStatus.PartiallySynced;
            }

            if (_period == null && _limit == null)
                Status = SyncStatus.Synced;
        }
    }
}
