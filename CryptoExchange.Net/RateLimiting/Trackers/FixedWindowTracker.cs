using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class FixedWindowTracker : IWindowTracker
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
        protected Queue<LimitEntry> _entries;

        private int _currentWeight = 0;

        /// <summary>
        /// Additional wait time to apply to account for time offset between server and client
        /// </summary>
        private static TimeSpan _fixedWindowBuffer = TimeSpan.FromMilliseconds(1000);

        public FixedWindowTracker(int limit, TimeSpan period)
        {
            Limit = limit;
            TimePeriod = period;
            _entries = new Queue<LimitEntry>();
        }

        public TimeSpan GetWaitTime(int weight)
        {
            // Remove requests no longer in time period from the history
            var checkTime = DateTime.UtcNow;
            RemoveBefore(checkTime.AddTicks(-(checkTime.Ticks % TimePeriod.Ticks)));

            if (Current + weight > Limit)
            {
                if (Current == 0)
                {
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");
                }

                // Wait until the next entry should be removed from the history
                return DetermineWaitTime();
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
            _entries.Enqueue(new LimitEntry(DateTime.UtcNow, weight));
        }

        /// <summary>
        /// Remove items before a certain type
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

        private TimeSpan DetermineWaitTime()
        {
            var checkTime = DateTime.UtcNow;
            var startCurrentWindow = checkTime.AddTicks(-(checkTime.Ticks % TimePeriod.Ticks));
            var wait = startCurrentWindow.Add(TimePeriod) - checkTime;
            return wait.Add(_fixedWindowBuffer);
        }
    }
}
