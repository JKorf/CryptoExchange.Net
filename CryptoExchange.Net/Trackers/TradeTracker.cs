using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers
{
    /// <summary>
    /// A tracker for trades on a symbol
    /// </summary>
    public abstract class TradeTracker
    {
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
        /// The total number of trades
        /// </summary>
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

        /// <summary>
        /// The timestamp of the first item
        /// </summary>
        protected DateTime _firstTimestamp;

        /// <summary>
        /// From which timestamp the trades are registered
        /// </summary>
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
        /// <summary>
        /// The average price across all trades
        /// </summary>
        /// <returns></returns>
        public decimal AveragePrice()
        {
            return Math.Round(GetData().Select(d => d.Price).DefaultIfEmpty().Average(), 8);
        }

        /// <summary>
        /// The average price in the last period
        /// </summary>
        /// <param name="period">Period to get the average price for</param>
        /// <returns></returns>
        public decimal AveragePriceForLast(TimeSpan period)
        {
            if (period > _period)
                throw new Exception("Can't calculate average price over period bigger than the tracker period limit");

            var compareTime = DateTime.UtcNow - period;
            return Math.Round(GetData(compareTime).Select(d => d.Price).DefaultIfEmpty().Average(), 8);
        }

        /// <summary>
        /// The average price since a specific time timestamp
        /// </summary>
        /// <param name="timestamp">Timestamp to get the average price since</param>
        /// <returns></returns>
        public decimal AveragePriceSince(DateTime timestamp)
        {
            return Math.Round(GetData(timestamp).Select(d => d.Price).DefaultIfEmpty().Average(), 8);
        }

        /// <summary>
        /// The trade volume across all trades
        /// </summary>
        /// <returns></returns>
        public decimal Volume()
        {
            return Math.Round(GetData().Sum(d => d.Quantity), 8);
        }

        /// <summary>
        /// The trade volume in the last time period
        /// </summary>
        /// <param name="period">Period to get the trade volume for</param>
        /// <returns></returns>
        public decimal VolumeForLast(TimeSpan period)
        {
            if (period > _period)
                throw new Exception("Can't calculate volume over period bigger than the tracker period limit");

            var compareTime = DateTime.UtcNow - period;
            return Math.Round(GetData(compareTime).Sum(d => d.Quantity), 8);
        }

        /// <summary>
        /// The trade volume since a specific time timestamp
        /// </summary>
        /// <param name="timestamp">Timestamp to get the trade volume since</param>
        /// <returns></returns>
        public decimal VolumeSince(DateTime timestamp)
        {
            return Math.Round(GetData(timestamp).Sum(d => d.Quantity), 8);
        }


        /// <summary>
        /// The qoute volume across all trades
        /// </summary>
        /// <returns></returns>
        public decimal QuoteVolume()
        {
            return Math.Round(GetData().Sum(d => d.Quantity * d.Price), 8);
        }

        /// <summary>
        /// The quote volume in the last time period
        /// </summary>
        /// <param name="period">Period to get the quote volume for</param>
        /// <returns></returns>
        public decimal QuoteVolumeForLast(TimeSpan period)
        {
            if (period > _period)
                throw new Exception("Can't calculate volume over period bigger than the tracker period limit");

            var compareTime = DateTime.UtcNow - period;
            return Math.Round(GetData(compareTime).Sum(d => d.Quantity * d.Price), 8);
        }

        /// <summary>
        /// The quote volume since a specific time timestamp
        /// </summary>
        /// <param name="timestamp">Timestamp to get the quote volume since</param>
        /// <returns></returns>
        public decimal QuoteVolumeSince(DateTime timestamp)
        {
            return Math.Round(GetData(timestamp).Sum(d => d.Quantity * d.Price), 8);
        }

        /// <summary>
        /// Event for when the initial snapshot is set
        /// </summary>
        public event Func<IEnumerable<ITradeItem>, Task>? SnapshotSet;
        /// <summary>
        /// Event for when a new trade is added
        /// </summary>
        public event Func<ITradeItem, Task>? Added;
        /// <summary>
        /// Event for when a trade is removed because it's no longer within the period/limit window
        /// </summary>
        public event Func<ITradeItem, Task>? Removed;

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

        /// <summary>
        /// Start synchronization
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            _logger.LogInformation("Starting trade tracker for {Symbol}", _symbol);
            var success = await DoStartAsync().ConfigureAwait(false);
            if (!success)
                _logger.LogWarning("Failed to start trade tracker for {Symbol}: {Error}", _symbol, success.Error);

            _updateSubscription = success.Data;
            _logger.LogInformation("Started trade tracker for {Symbol}", _symbol);
        }

        /// <summary>
        /// Stop synchronization
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping trade tracker for {Symbol}", _symbol);
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

        /// <summary>
        /// Get the data tracked
        /// </summary>
        /// <param name="since">Return data after his timestamp</param>
        /// <returns></returns>
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
                SnapshotSet?.Invoke(GetData());
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
                    Added?.Invoke(item);
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
                        Removed?.Invoke(item.Value);
                }
            }

            if (_limit != null && _data.Count > _limit.Value)
            {
                var toRemove = _data.Count - _limit.Value;
                foreach (var item in _data.OrderBy(d => d.Value.Timestamp).Take(toRemove))
                {
                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        Removed?.Invoke(item.Value);
                }
            }

            _lastWindowApplied = DateTime.UtcNow;
            _changed = false;
        }
    }
}
