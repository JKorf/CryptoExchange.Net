using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    public abstract class WindowTracker
    {
        public TimeSpan Timeperiod { get; }
        public int Limit { get; }
        public int Current => _currentWeight;

        protected List<LimitEntry> _entries;
        protected int _currentWeight = 0;

        public WindowTracker(int limit, TimeSpan period)
        {
            Limit = limit;
            Timeperiod = period;
            _entries = new List<LimitEntry>();
        }

        public abstract TimeSpan ProcessTopic(int weight);

        public void AddEntry(int weight)
        {
            _currentWeight += weight;
            _entries.Add(new LimitEntry(DateTime.UtcNow, weight));
        }

        protected void RemoveBefore(DateTime time)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Timestamp < time)
                {
                    var entry = _entries[i];
                    _entries.Remove(entry);
                    _currentWeight -= entry.Weight;
                    i--;
                }
                else
                    break;
            }
        }

        protected internal struct LimitEntry
        {
            public DateTime Timestamp { get; set; }
            public int Weight { get; set; }

            public LimitEntry(DateTime timestamp, int weight)
            {
                Timestamp = timestamp;
                Weight = weight;
            }
        }
    }
}
