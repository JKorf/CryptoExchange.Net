using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Klines
{
    /// <inheritdoc />
    public class KlineTracker : IKlineTracker
    {
        private readonly IKlineSocketClient _socketClient;
        private readonly IKlineRestClient _restClient;
        private SyncStatus _status;
        private bool _startWithSnapshot;

        /// <summary>
        /// The internal data structure
        /// </summary>
        protected readonly SortedDictionary<DateTime, SharedKline> _data = new SortedDictionary<DateTime, SharedKline>();
        /// <summary>
        /// The pre-snapshot queue buffering updates received before the snapshot is set and which will be applied after the snapshot was set
        /// </summary>
        protected readonly List<SharedKline> _preSnapshotQueue = new List<SharedKline>();
        /// <summary>
        /// Lock for accessing _data
        /// </summary>
        protected readonly object _lock = new object();
        /// <summary>
        /// The last time the window was applied
        /// </summary>
        protected DateTime _lastWindowApplied = DateTime.MinValue;
        /// <summary>
        /// Whether or not the data has changed since last window was applied
        /// </summary>
        protected bool _changed = false;
        /// <summary>
        /// The kline interval
        /// </summary>
        protected readonly SharedKlineInterval _interval;
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
                _logger.KlineTrackerStatusChanged(SymbolName, old, value);
                OnStatusChanged?.Invoke(old, _status);
            }
        }

        /// <inheritdoc />
        public string Exchange { get; }

        /// <inheritdoc />
        public string SymbolName { get; }

        /// <inheritdoc />
        public SharedSymbol Symbol { get; }

        /// <inheritdoc/>
        public int? Limit { get; }
        /// <inheritdoc/>
        public TimeSpan? Period { get; }

        /// <inheritdoc />
        public DateTime? SyncedFrom
        {
            get
            {
                if (Period == null)
                    return _firstTimestamp;

                var max = DateTime.UtcNow - Period.Value;
                if (_firstTimestamp > max)
                    return _firstTimestamp;

                return max;
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
        public SharedKline? Last
        {
            get
            {
                lock (_lock)
                {
                    ApplyWindow(true);
                    return _data.LastOrDefault().Value;
                }
            }
        }

        /// <inheritdoc />
        public event Func<SharedKline, Task>? OnAdded;
        /// <inheritdoc />
        public event Func<SharedKline, Task>? OnUpdated;
        /// <inheritdoc />
        public event Func<SharedKline, Task>? OnRemoved;
        /// <inheritdoc />
        public event Func<SyncStatus, SyncStatus, Task>? OnStatusChanged;

        /// <summary>
        /// ctor
        /// </summary>
        public KlineTracker(
            ILogger? logger,
            IKlineRestClient restClient,
            IKlineSocketClient socketClient,
            SharedSymbol symbol,
            SharedKlineInterval interval,
            int? limit = null,
            TimeSpan? period = null)
        {
            _logger = logger ?? new NullLogger<KlineTracker>();
            Symbol = symbol;
            SymbolName = socketClient.FormatSymbol(symbol.BaseAsset, symbol.QuoteAsset, symbol.TradingMode, symbol.DeliverTime);
            Exchange = restClient.Exchange;
            Limit = limit;
            Period = period;
            _interval = interval;
            _socketClient = socketClient;
            _restClient = restClient;
        }

        /// <inheritdoc />
        public async Task<CallResult> StartAsync(bool startWithSnapshot = true)
        {
            if (Status != SyncStatus.Disconnected)
                throw new InvalidOperationException($"Can't start syncing unless state is {SyncStatus.Disconnected}. Current state: {Status}");

            _startWithSnapshot = startWithSnapshot;
            Status = SyncStatus.Syncing;
            _logger.KlineTrackerStarting(SymbolName);

            var startResult = await DoStartAsync().ConfigureAwait(false);
            if (!startResult)
            {
                _logger.KlineTrackerStartFailed(SymbolName, startResult.Error!.ToString());
                Status = SyncStatus.Disconnected;
                return new CallResult(startResult.Error!);
            }

            _updateSubscription = startResult.Data;
            _updateSubscription.ConnectionLost += HandleConnectionLost;
            _updateSubscription.ConnectionClosed += HandleConnectionClosed;
            _updateSubscription.ConnectionRestored += HandleConnectionRestored;
            Status = SyncStatus.Synced;
            _logger.KlineTrackerStarted(SymbolName);
            return CallResult.SuccessResult;
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            _logger.KlineTrackerStopping(SymbolName);
            Status = SyncStatus.Disconnected;
            await DoStopAsync().ConfigureAwait(false);
            _data.Clear();
            _preSnapshotQueue.Clear();
            _logger.KlineTrackerStopped(SymbolName);
        }

        /// <summary>
        /// The start procedure needed for kline syncing, generally subscribing to an update stream and requesting the snapshot
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<CallResult<UpdateSubscription>> DoStartAsync()
        {
            var subResult = await _socketClient.SubscribeToKlineUpdatesAsync(new SubscribeKlineRequest(Symbol, _interval),
                 update =>
                 {
                     AddOrUpdate(update.Data);
                 }).ConfigureAwait(false);

            if (!subResult)
            {
                Status = SyncStatus.Disconnected;
                return subResult;
            }

            if (!_startWithSnapshot)
                return subResult;

            var startTime = Period == null ? (DateTime?)null : DateTime.UtcNow.Add(-Period.Value);
            if (_restClient.GetKlinesOptions.MaxAge != null && DateTime.UtcNow.Add(-_restClient.GetKlinesOptions.MaxAge.Value) > startTime)
                startTime = DateTime.UtcNow.Add(-_restClient.GetKlinesOptions.MaxAge.Value);

            var limit = Math.Min(_restClient.GetKlinesOptions.MaxLimit, Limit ?? 100);

            var request = new GetKlinesRequest(Symbol, _interval, startTime, DateTime.UtcNow, limit: limit);
            var data = new List<SharedKline>();
            await foreach (var result in ExchangeHelpers.ExecutePages(_restClient.GetKlinesAsync, request).ConfigureAwait(false))
            {
                if (!result)
                {
                    _ = subResult.Data.CloseAsync();
                    Status = SyncStatus.Disconnected;
                    return subResult.AsError<UpdateSubscription>(result.Error!);
                }

                if (Limit != null && data.Count > Limit)
                    break;

                data.AddRange(result.Data);
            }

            SetInitialData(data);
            return subResult;
        }

        /// <summary>
        /// The stop procedure needed, generally stopping the update stream
        /// </summary>
        /// <returns></returns>
        protected virtual Task DoStopAsync() => _updateSubscription?.CloseAsync() ?? Task.CompletedTask;

        /// <inheritdoc />
        public KlinesStats GetStats(DateTime? fromTimestamp = null, DateTime? toTimestamp = null)
        {
            var compareTime = SyncedFrom?.AddSeconds(-2);
            var stats = GetStats(GetData(fromTimestamp, toTimestamp));
            stats.Complete = (fromTimestamp == null || fromTimestamp >= compareTime) && (toTimestamp == null || toTimestamp >= compareTime);
            return stats;
        }

        private KlinesStats GetStats(IEnumerable<SharedKline> klines)
        {
            if (!klines.Any())
                return new KlinesStats();

            return new KlinesStats
            {
                KlineCount = klines.Count(),
                FirstOpenTime = klines.First().OpenTime,
                LastOpenTime = klines.Last().OpenTime,
                HighPrice = klines.Select(d => d.LowPrice).Max(),
                LowPrice = klines.Select(d => d.HighPrice).Min(),
                Volume = klines.Select(d => d.Volume).Sum(),
                AverageVolume = Math.Round(klines.OrderByDescending(d => d.OpenTime).Skip(1).Select(d => d.Volume).DefaultIfEmpty().Average(), 8)
            };
        }

        /// <inheritdoc />
        public SharedKline[] GetData(DateTime? since = null, DateTime? until = null)
        {
            lock (_lock)
            {
                ApplyWindow(true);

                IEnumerable<SharedKline> result = _data.Values;
                if (since != null)
                    result = result.Where(d => d.OpenTime >= since);
                if (until != null)
                    result = result.Where(d => d.OpenTime <= until);

                return result.ToArray();
            }
        }

        /// <summary>
        /// Set the initial kline data snapshot
        /// </summary>
        /// <param name="data"></param>
        protected void SetInitialData(IEnumerable<SharedKline> data)
        {
            lock (_lock)
            {
                _data.Clear();

                IEnumerable<SharedKline> items = data.OrderByDescending(d => d.OpenTime);
                if (Limit != null)
                    items = items.Take(Limit.Value);
                if (Period != null)
                    items = items.Where(e => e.OpenTime >= DateTime.UtcNow.Add(-Period.Value));

                foreach (var item in items.OrderBy(d => d.OpenTime))
                    _data.Add(item.OpenTime, item);

                _snapshotSet = true;

                foreach (var item in _preSnapshotQueue)
                {
                    if (_data.ContainsKey(item.OpenTime))
                        continue;

                    _data.Add(item.OpenTime, item);
                }

                _firstTimestamp = _data.Min(v => v.Key);
                ApplyWindow(false);
                _logger.KlineTrackerInitialDataSet(SymbolName, _data.Last().Key);
            }
        }

        /// <summary>
        /// Add or update a kline
        /// </summary>
        /// <param name="item"></param>
        protected void AddOrUpdate(SharedKline item) => AddOrUpdate(new[] { item });

        /// <summary>
        /// Add or update klines
        /// </summary>
        /// <param name="items"></param>
        protected void AddOrUpdate(IEnumerable<SharedKline> items)
        {
            lock (_lock)
            {
                if (_restClient != null && _startWithSnapshot && !_snapshotSet)
                {
                    _preSnapshotQueue.AddRange(items);
                    return;
                }

                foreach (var item in items)
                {
                    if (_data.TryGetValue(item.OpenTime, out var existing))
                    {
                        _data.Remove(item.OpenTime);
                        _data.Add(item.OpenTime, item);
                        OnUpdated?.Invoke(item);
                        _logger.KlineTrackerKlineUpdated(SymbolName, _data.Last().Key);
                    }
                    else
                    {
                        _data.Add(item.OpenTime, item);
                        OnAdded?.Invoke(item);
                        _logger.KlineTrackerKlineAdded(SymbolName, _data.Last().Key);
                    }
                }

                _firstTimestamp = _data.Min(x => x.Key);
                _changed = true;

                SetSyncStatus();
                ApplyWindow(true);
            }
        }

        private void ApplyWindow(bool broadcastEvents)
        {
            if (!_changed && (DateTime.UtcNow - _lastWindowApplied) < TimeSpan.FromSeconds(1))
                return;

            if (Period != null)
            {
                var compareDate = DateTime.UtcNow.Add(-Period.Value);
                for (var i = 0; i < _data.Count; i++)
                {
                    var item = _data.ElementAt(0);
                    if (item.Key >= compareDate)
                        break;

                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        OnRemoved?.Invoke(item.Value);
                }
            }

            if (Limit != null && _data.Count > Limit.Value)
            {
                var toRemove = Math.Max(0, _data.Count - Limit.Value);
                for (var i = 0; i < toRemove; i++)
                {
                    var item = _data.ElementAt(0);
                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        OnRemoved?.Invoke(item.Value);
                }
            }

            _lastWindowApplied = DateTime.UtcNow;
            _changed = false;
        }

        private void HandleConnectionLost()
        {
            _logger.KlineTrackerConnectionLost(SymbolName);
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
            _logger.KlineTrackerConnectionClosed(SymbolName);
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

            _logger.KlineTrackerConnectionRestored(SymbolName);
            SetSyncStatus();
        }

        private void SetSyncStatus()
        {
            if (Status == SyncStatus.Synced)
                return;

            if (Period != null)
            {
                if (_firstTimestamp <= DateTime.UtcNow - Period.Value)
                    Status = SyncStatus.Synced;
                else
                    Status = SyncStatus.PartiallySynced;
            }

            if (Limit != null)
            {
                if (_data.Count == Limit.Value)
                    Status = SyncStatus.Synced;
                else
                    Status = SyncStatus.PartiallySynced;
            }

            if (Period == null && Limit == null)
                Status = SyncStatus.Synced;
        }
    }
}
