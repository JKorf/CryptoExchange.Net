using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers
{
    public abstract class KlineTracker
    {
        protected readonly Dictionary<DateTime, IKlineItem> _data = new Dictionary<DateTime, IKlineItem>();
        protected readonly List<IKlineItem> _preSnapshotQueue = new List<IKlineItem>();
        protected readonly object _lock = new object();
        protected readonly int? _limit;
        protected readonly TimeSpan? _period;
        protected bool _snapshotSet;

        /// <summary>
        /// The total number of trades
        /// </summary>
        public int Count => _data.Count;
        public decimal LowPrice => GetData().Min(d => d.LowPrice);
        public decimal HighPrice => GetData().Max(d => d.HighPrice);

        public event Func<IEnumerable<IKlineItem>, Task> SnapshotSet;
        public event Func<IKlineItem, Task> Added;
        public event Func<IKlineItem, Task> Updated;
        public event Func<IKlineItem, Task> Removed;

        public KlineTracker(int? limit = null, TimeSpan? period = null)
        {
            _limit = limit;
            _period = period;
        }
        public async Task StartAsync()
        {
            await DoStartAsync();
        }

        public async Task StopAsync()
        {
            await DoStopAsync();
        }

        protected abstract Task<CallResult> DoStartAsync();
        protected abstract Task DoStopAsync();


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

        protected void AddData(IKlineItem item) => AddData(new[] { item });

        protected void AddData(IEnumerable<IKlineItem> items)
        {
            lock (_lock)
            {
                if (!_snapshotSet)
                {
                    _preSnapshotQueue.AddRange(items);
                    return;
                }

                foreach (var item in items)
                    AddOrUpdate(item);

                ApplyWindow(true);
            }
        }

        protected void AddOrUpdate(IKlineItem item)
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

        protected void ApplyWindow(bool broadcastEvents)
        {
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
                foreach (var item in _data.OrderBy(d => d.Key).Take(_data.Count - _limit.Value))
                {
                    _data.Remove(item.Key);
                    if (broadcastEvents)
                        Removed?.Invoke(item.Value);
                }
            }
        }
    }
}
