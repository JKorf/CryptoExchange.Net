using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Trades
{
    /// <inheritdoc />
    public abstract class TradeTracker : ITradeTracker
    {
        private SyncStatus _status;

        /// <summary>
        /// The internal data structure
        /// </summary>
        protected readonly Dictionary<string, ITradeItem> _data = new Dictionary<string, ITradeItem>();
        /// <summary>
        /// The pre-snapshot queue buffering updates received before the snapshot is set and which will be applied after the snapshot was set
        /// </summary>
        protected readonly List<ITradeItem> _preSnapshotQueue = new List<ITradeItem>();
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
        protected readonly string _symbol;
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
        protected DateTime _firstTimestamp;

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
                _logger.Log(LogLevel.Information, "Trade tracker for {Symbol} status changed: {old} => {value}", _symbol, old, value);
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
        public DateTime SyncedFrom
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
        public decimal AveragePrice()
        {
            return Math.Round(GetData().Select(d => d.Price).DefaultIfEmpty().Average(), 8);
        }

        /// <inheritdoc />
        public decimal AveragePriceForLast(TimeSpan period)
        {
            if (period > _period)
                throw new Exception("Can't calculate average price over period bigger than the tracker period limit");

            var compareTime = DateTime.UtcNow - period;
            return Math.Round(GetData(compareTime).Select(d => d.Price).DefaultIfEmpty().Average(), 8);
        }

        /// <inheritdoc />
        public decimal AveragePriceSince(DateTime timestamp)
        {
            return Math.Round(GetData(timestamp).Select(d => d.Price).DefaultIfEmpty().Average(), 8);
        }

        /// <inheritdoc />
        public decimal Volume()
        {
            return Math.Round(GetData().Sum(d => d.Quantity), 8);
        }

        /// <inheritdoc />
        public decimal VolumeForLast(TimeSpan period)
        {
            if (period > _period)
                throw new Exception("Can't calculate volume over period bigger than the tracker period limit");

            var compareTime = DateTime.UtcNow - period;
            return Math.Round(GetData(compareTime).Sum(d => d.Quantity), 8);
        }

        /// <inheritdoc />
        public decimal VolumeSince(DateTime timestamp)
        {
            return Math.Round(GetData(timestamp).Sum(d => d.Quantity), 8);
        }

        /// <inheritdoc />
        public decimal QuoteVolume()
        {
            return Math.Round(GetData().Sum(d => d.Quantity * d.Price), 8);
        }

        /// <inheritdoc />
        public decimal QuoteVolumeForLast(TimeSpan period)
        {
            if (period > _period)
                throw new Exception("Can't calculate volume over period bigger than the tracker period limit");

            var compareTime = DateTime.UtcNow - period;
            return Math.Round(GetData(compareTime).Sum(d => d.Quantity * d.Price), 8);
        }

        /// <inheritdoc />
        public decimal QuoteVolumeSince(DateTime timestamp)
        {
            return Math.Round(GetData(timestamp).Sum(d => d.Quantity * d.Price), 8);
        }

        /// <inheritdoc />
        public event Func<IEnumerable<ITradeItem>, Task>? OnSnapshotSet;
        /// <inheritdoc />
        public event Func<ITradeItem, Task>? OnAdded;
        /// <inheritdoc />
        public event Func<ITradeItem, Task>? OnRemoved;
        /// <inheritdoc />
        public event Func<SyncStatus, SyncStatus, Task>? OnStatusChanged;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="logger"></param>
        /// <param name="symbol"></param>
        /// <param name="period"></param>
        public TradeTracker(ILogger? logger, string symbol, int? limit = null, TimeSpan? period = null)
        {
            _logger = logger ?? new TraceLogger();
            _symbol = symbol;
            _limit = limit;
            _period = period;
        }

        /// <inheritdoc />
        public async Task StartAsync()
        {
            if (Status != SyncStatus.Disconnected)
                throw new InvalidOperationException($"Can't start syncing unless state is {SyncStatus.Disconnected}. Current state: {Status}");

            Status = SyncStatus.Syncing;
            _logger.LogInformation("Starting trade tracker for {Symbol}", _symbol);
            var success = await DoStartAsync().ConfigureAwait(false);
            if (!success)
            {
                _logger.LogWarning("Failed to start trade tracker for {Symbol}: {Error}", _symbol, success.Error);
                Status = SyncStatus.Disconnected;
                return;
            }

            _updateSubscription = success.Data;
            _updateSubscription.ConnectionLost += HandleConnectionLost;
            _updateSubscription.ConnectionClosed += HandleConnectionClosed;
            _updateSubscription.ConnectionRestored += HandleConnectionRestored;
            Status = SyncStatus.Synced;
            _logger.LogInformation("Started trade tracker for {Symbol}", _symbol);
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping trade tracker for {Symbol}", _symbol);
            Status = SyncStatus.Disconnected;
            await DoStopAsync().ConfigureAwait(false);
            _data.Clear();
            _preSnapshotQueue.Clear();
            _logger.LogInformation("Stopped trade tracker for {Symbol}", _symbol);
        }

        /// <summary>
        /// The start procedure needed for trade syncing, generally subscribing to an update stream and requesting the snapshot
        /// </summary>
        /// <returns></returns>
        protected abstract Task<CallResult<UpdateSubscription>> DoStartAsync();

        /// <summary>
        /// The stop procedure needed, generally stopping the update stream
        /// </summary>
        /// <returns></returns>
        protected virtual Task DoStopAsync() => _updateSubscription?.CloseAsync() ?? Task.CompletedTask;

        /// <inheritdoc />
        public IEnumerable<ITradeItem> GetData(DateTime? since = null)
        {
            lock (_lock)
            {
                ApplyWindow(true);

                IEnumerable<ITradeItem> result = _data.Values;
                if (since != null)
                    result = result.Where(d => d.Timestamp >= since);

                return result.ToList();
            }
        }

        /// <summary>
        /// Set the initial trade data snapshot
        /// </summary>
        /// <param name="data"></param>
        protected void SetInitialData(IEnumerable<ITradeItem> data)
        {
            lock (_lock)
            {
                _data.Clear();

                IEnumerable<ITradeItem> items = data.OrderByDescending(d => d.Timestamp);
                if (_limit != null)
                    items = items.Take(_limit.Value);
                if (_period != null)
                    items = items.Where(e => e.Timestamp >= DateTime.UtcNow.Add(-_period.Value));

                foreach (var item in items.OrderBy(d => d.Timestamp))
                    _data.Add(item.Id, item);

                _snapshotSet = true;
                _changed = true;

                _logger.LogTrace("Snapshot set, last id: {LastId}", _data.Last().Key);

                foreach (var item in _preSnapshotQueue)
                {
                    if (_data.ContainsKey(item.Id))
                    {
                        Debug.WriteLine($"Skipping {item.Id}, already in snapshot");
                        _logger.LogTrace("Skipping {Id}, already in snapshot", item.Id);
                        continue;
                    }

                    _logger.LogTrace("Adding {item.Id} from pre-snapshot", item.Id);
                    _data.Add(item.Id, item);
                }

                _firstTimestamp = _data.Values.Min(v => v.Timestamp);

                ApplyWindow(false);
                OnSnapshotSet?.Invoke(GetData());
            }
        }

        /// <summary>
        /// Add a trade
        /// </summary>
        /// <param name="item"></param>
        protected void AddData(ITradeItem item) => AddData(new[] { item });

        /// <summary>
        /// Add a list of trades
        /// </summary>
        /// <param name="items"></param>
        protected void AddData(IEnumerable<ITradeItem> items)
        {
            lock (_lock)
            {
                if (!_snapshotSet)
                {
                    _preSnapshotQueue.AddRange(items);
                    return;
                }

                foreach (var item in items)
                {
                    _logger.LogTrace("Adding {item.Id}", item.Id);
                    _data.Add(item.Id, item);
                    OnAdded?.Invoke(item);
                }

                _changed = true;

                // Check if we need to apply window. To save processing cost the window is only applied every 30 seconds
                // or when data is requested
                if (DateTime.UtcNow - _lastWindowApplied > TimeSpan.FromSeconds(5))
                    ApplyWindow(true);
            }
        }

        private void ApplyWindow(bool broadcastEvents)
        {
            if (!_changed)
                return;

            _logger.LogTrace("Applying window");

            if (_period != null)
            {
                var compareDate = DateTime.UtcNow.Add(-_period.Value);
                foreach (var item in _data.Where(d => d.Value.Timestamp < compareDate))
                {
                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        OnRemoved?.Invoke(item.Value);
                }
            }

            if (_limit != null && _data.Count > _limit.Value)
            {
                var toRemove = _data.Count - _limit.Value;
                foreach (var item in _data.OrderBy(d => d.Value.Timestamp).Take(toRemove))
                {
                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        OnRemoved?.Invoke(item.Value);
                }

                if (_period == null)
                    _firstTimestamp = _data.Min(d => d.Value.Timestamp);
            }

            _lastWindowApplied = DateTime.UtcNow;
            _changed = false;
        }


        private void HandleConnectionLost()
        {
            _logger.Log(LogLevel.Warning, "Trade tracker for {Symbol} connection lost", _symbol);
            if (Status != SyncStatus.Disconnected)
            {
                Status = SyncStatus.Syncing;
                _snapshotSet = false;
                _preSnapshotQueue.Clear();
            }
        }

        private void HandleConnectionClosed()
        {
            _logger.Log(LogLevel.Warning, "Trade tracker for {Symbol} disconnected", _symbol);
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

            _logger.Log(LogLevel.Information, "Trade tracker for {Symbol} successfully resynchronized", _symbol);
            Status = SyncStatus.Synced;
        }

    }
}
