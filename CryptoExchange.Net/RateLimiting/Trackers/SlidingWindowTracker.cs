using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class SlidingWindowTracker : IWindowTracker
    {
        /// <summary>
        /// The time period for this tracker
        /// </summary>
        public TimeSpan TimePeriod { get; }
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

        public SlidingWindowTracker(int limit, TimeSpan period)
        {
            Limit = limit;
            TimePeriod = period;
            _entries = new List<LimitEntry>();
        }

        public TimeSpan GetWaitTime(int weight)
        {
            // Remove requests no longer in time period from the history
            RemoveBefore(DateTime.UtcNow - TimePeriod);

            if (Current + weight > Limit)
            {
                if (Current == 0)
                {
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");
                }

                // Wait until the next entry should be removed from the history
                return DetermineWaitTime(weight);
            }

            return TimeSpan.Zero;
        }

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

        private TimeSpan DetermineWaitTime(int requestWeight)
        {
            var weightToRemove = Math.Max(Current - (Limit - requestWeight), 0);
            var removedWeight = 0;
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                removedWeight += entry.Weight;
                if (removedWeight >= weightToRemove)
                {
                    return entry.Timestamp + TimePeriod - DateTime.UtcNow;
                }
            }

            throw new Exception("Request not possible to execute with current rate limit guard. " +
                        $" Request weight: {requestWeight}, Ratelimit: {Limit}");
        }
    }
}
