using System;
using System.Collections.Generic;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class FixedAfterStartWindowTracker : IWindowTracker
    {
        /// <inheritdoc />
        public TimeSpan TimePeriod { get; }
        /// <inheritdoc />
        public int Limit { get; }
        /// <inheritdoc />
        public int Current => _currentWeight;

        private readonly Queue<LimitEntry> _entries;
        private int _currentWeight = 0;
        private DateTime? _nextReset;

        /// <summary>
        /// Additional wait time to apply to account for time offset between server and client
        /// </summary>
        private static TimeSpan _fixedWindowBuffer = TimeSpan.FromMilliseconds(1000);

        public FixedAfterStartWindowTracker(int limit, TimeSpan period)
        {
            Limit = limit;
            TimePeriod = period;
            _entries = new Queue<LimitEntry>();
        }

        public TimeSpan GetWaitTime(int weight)
        {
            // Remove requests no longer in time period from the history
            var checkTime = DateTime.UtcNow;
            if (_nextReset != null && checkTime > _nextReset)
                RemoveBefore(_nextReset.Value);

            if (Current == 0)
                _nextReset = null;

            if (Current + weight > Limit)
            {
                // The weight would cause the rate limit to be passed
                if (Current == 0)
                {
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");
                }

                // Determine the time to wait before this weight can be applied without going over the rate limit
                return DetermineWaitTime();
            }

            // Weight can fit without going over limit
            return TimeSpan.Zero;
        }

        /// <inheritdoc />
        public void ApplyWeight(int weight)
        {
            if (_currentWeight == 0)
                _nextReset = DateTime.UtcNow + TimePeriod;
            _currentWeight += weight;
            _entries.Enqueue(new LimitEntry(DateTime.UtcNow, weight));
        }

        /// <summary>
        /// Remove items before a certain time
        /// </summary>
        /// <param name="time"></param>
        protected void RemoveBefore(DateTime time)
        {
            while (true)
            {
                if (_entries.Count == 0)
                    break;

                var firstItem = _entries.Peek();
                if (firstItem.Timestamp < time)
                {
                    _entries.Dequeue();
                    _currentWeight -= firstItem.Weight;
                }
                else
                {
                    // Either no entries left, or the entry time is still within the window
                    break;
                }
            }
        }

        /// <summary>
        /// Determine the time to wait before a new item would fit
        /// </summary>
        /// <returns></returns>
        private TimeSpan DetermineWaitTime()
        {
            var checkTime = DateTime.UtcNow;
            var result = (_nextReset!.Value - checkTime) + _fixedWindowBuffer;
            if (result < TimeSpan.Zero)
                return TimeSpan.Zero;
            return result;
        }
    }
}
