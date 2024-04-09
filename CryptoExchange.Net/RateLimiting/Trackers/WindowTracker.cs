using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    /// <summary>
    /// Window tracker
    /// </summary>
    public abstract class WindowTracker
    {
        /// <summary>
        /// The time period for this tracker
        /// </summary>
        public TimeSpan Timeperiod { get; }
        /// <summary>
        /// Limit for this tracker
        /// </summary>
        public int Limit { get; }
        /// <summary>
        /// Current
        /// </summary>
        public int Current => _currentWeight;
        /// <summary>
        /// Rate limit entries
        /// </summary>
        protected List<LimitEntry> _entries;

        private int _currentWeight = 0;

        /// <summary>
        /// New window tracker
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        /// <param name="initialCount"></param>
        public WindowTracker(int limit, TimeSpan period, int initialCount)
        {
            Limit = limit;
            Timeperiod = period;
            _entries = new List<LimitEntry>();
            if (initialCount != 0)
            {
                _entries.Add(new LimitEntry(DateTime.UtcNow, initialCount));
                _currentWeight += initialCount;
            }
        }

        /// <summary>
        /// Process a new item
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        public abstract TimeSpan GetWaitTime(int weight);

        /// <summary>
        /// Apply a new weighted item
        /// </summary>
        /// <param name="weight"></param>
        public void ApplyWeight(int weight)
        {
            _currentWeight += weight;
            _entries.Add(new LimitEntry(DateTime.UtcNow, weight));
        }

        /// <summary>
        /// Remove items before a certain type
        /// </summary>
        /// <param name="time"></param>
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
                {
                    break;
                }
            }
        }
    }
}
