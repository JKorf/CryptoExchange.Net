using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Klines
{
    /// <inheritdoc />
    public abstract class KlineTracker : IKlineTracker
    {
        private SyncStatus _status;

        /// <summary>
        /// The internal data structure
        /// </summary>
        protected readonly Dictionary<DateTime, IKlineItem> _data = new Dictionary<DateTime, IKlineItem>();
        /// <summary>
        /// The pre-snapshot queue buffering updates received before the snapshot is set and which will be applied after the snapshot was set
        /// </summary>
        protected readonly List<IKlineItem> _preSnapshotQueue = new List<IKlineItem>();
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
        /// The last time the window was applied
        /// </summary>
        protected DateTime _lastWindowApplied = DateTime.MinValue;
        /// <summary>
        /// Whether or not the data has changed since last window was applied
        /// </summary>
        protected bool _changed = false;
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
        public decimal LowPrice() => GetData().Select(d => d.LowPrice).DefaultIfEmpty().Min();

        /// <inheritdoc />
        public decimal HighPrice() => GetData().Select(d => d.HighPrice).DefaultIfEmpty().Max();

        /// <inheritdoc />
        public decimal Volume() => GetData().Select(d => d.Volume).Sum();

        /// <inheritdoc />
        public decimal AverageVolume() => GetData().OrderByDescending(d => d.OpenTime).Skip(1).Select(d => d.Volume).DefaultIfEmpty().Average();

        /// <inheritdoc />
        public event Func<IEnumerable<IKlineItem>, Task>? SnapshotSet;
        /// <inheritdoc />
        public event Func<IKlineItem, Task>? Added;
        /// <inheritdoc />
        public event Func<IKlineItem, Task>? Updated;
        /// <inheritdoc />
        public event Func<IKlineItem, Task>? Removed;
        /// <inheritdoc />
        public event Func<SyncStatus, SyncStatus, Task>? OnStatusChanged;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="symbol"></param>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        public KlineTracker(ILogger? logger, string symbol, int? limit = null, TimeSpan? period = null)
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
            _logger.LogInformation("Starting kline tracker for {Symbol}", _symbol);
            var success = await DoStartAsync().ConfigureAwait(false);
            if (!success)
            {
                _logger.LogWarning("Failed to start kline tracker for {Symbol}: {Error}", _symbol, success.Error);
                Status = SyncStatus.Disconnected;
                return;
            }

            _updateSubscription = success.Data;
            _updateSubscription.ConnectionLost += HandleConnectionLost;
            _updateSubscription.ConnectionClosed += HandleConnectionClosed;
            _updateSubscription.ConnectionRestored += HandleConnectionRestored;
            Status = SyncStatus.Synced;
            _logger.LogInformation("Started kline tracker for {Symbol}", _symbol);
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
        /// The start procedure needed for kline syncing, generally subscribing to an update stream and requesting the snapshot
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
        public IEnumerable<IKlineItem> GetData(DateTime? since = null)
        {
            lock (_lock)
            {
                ApplyWindow(true);

                IEnumerable<IKlineItem> result = _data.Values;
                if (since != null)
                    result = result.Where(d => d.OpenTime >= since);

                return result.ToList();
            }
        }

        /// <summary>
        /// Set the initial kline data snapshot
        /// </summary>
        /// <param name="data"></param>
        protected void SetInitialData(IEnumerable<IKlineItem> data)
        {
            lock (_lock)
            {
                _data.Clear();

                IEnumerable<IKlineItem> items = data.OrderByDescending(d => d.OpenTime);
                if (_limit != null)
                    items = items.Take(_limit.Value);
                if (_period != null)
                    items = items.Where(e => e.OpenTime >= DateTime.UtcNow.Add(-_period.Value));

                foreach (var item in items.OrderBy(d => d.OpenTime))
                    _data.Add(item.OpenTime, item);

                _snapshotSet = true;

                Debug.WriteLine("Snapshot set, last time: " + _data.Last().Key);


                foreach (var item in _preSnapshotQueue)
                {
                    if (_data.ContainsKey(item.OpenTime))
                    {
                        Debug.WriteLine($"Skipping {item.OpenTime}, already in snapshot");
                        continue;
                    }

                    Debug.WriteLine($"Adding {item.OpenTime} from pre-snapshot");
                    _data.Add(item.OpenTime, item);
                }

                ApplyWindow(false);
                SnapshotSet?.Invoke(GetData());
            }
        }

        /// <summary>
        /// Add or update a kline
        /// </summary>
        /// <param name="item"></param>
        protected void AddOrUpdate(IKlineItem item) => AddOrUpdate(new[] { item });

        /// <summary>
        /// Add or update klines
        /// </summary>
        /// <param name="items"></param>
        protected void AddOrUpdate(IEnumerable<IKlineItem> items)
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
                    if (_data.TryGetValue(item.OpenTime, out var existing))
                    {
                        Debug.WriteLine($"Replacing {item.OpenTime}");
                        _data.Remove(item.OpenTime);
                        _data.Add(item.OpenTime, item);
                        Updated?.Invoke(item);
                    }
                    else
                    {
                        Debug.WriteLine($"Adding {item.OpenTime}");
                        _data.Add(item.OpenTime, item);
                        Added?.Invoke(item);
                    }
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
                foreach (var item in _data.Where(d => d.Key < compareDate))
                {
                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        Removed?.Invoke(item.Value);
                }
            }

            if (_limit != null && _data.Count > _limit.Value)
            {
                var toRemove = _data.Count - _limit.Value;
                foreach (var item in _data.OrderBy(d => d.Key).Take(toRemove))
                {
                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        Removed?.Invoke(item.Value);
                }
            }
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
