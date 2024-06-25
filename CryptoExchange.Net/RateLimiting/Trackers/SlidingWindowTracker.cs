using System;
using System.Collections.Generic;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class SlidingWindowTracker : IWindowTracker
    {
        /// <inheritdoc />
        public TimeSpan TimePeriod { get; }
        /// <inheritdoc />
        public int Limit { get; }
        /// <inheritdoc />
        public int Current => _currentWeight;

        private readonly List<LimitEntry> _entries;
        private int _currentWeight = 0;

        /// <summary>
        /// Additional wait time to apply to account for fluctuating request times
        /// </summary>
        private static readonly TimeSpan _slidingWindowBuffer = TimeSpan.FromMilliseconds(1000);

        public SlidingWindowTracker(int limit, TimeSpan period)
        {
            Limit = limit;
            TimePeriod = period;
            _entries = new List<LimitEntry>();
        }

        /// <inheritdoc />
        public TimeSpan GetWaitTime(int weight)
        {
            // Remove requests no longer in time period from the history
            RemoveBefore(DateTime.UtcNow - TimePeriod);

            if (Current + weight > Limit)
            {
                // The weight would cause the rate limit to be passed
                if (Current == 0)
                {
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");
                }

                // Determine the time to wait before this weight can be applied without going over the rate limit
                return DetermineWaitTime(weight);
            }

            // Weight can fit without going over limit
            return TimeSpan.Zero;
        }

        /// <inheritdoc />
        public void ApplyWeight(int weight)
        {
            _currentWeight += weight;
            _entries.Add(new LimitEntry(DateTime.UtcNow, weight));
        }

        /// <summary>
        /// Remove items before a certain time
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

        /// <summary>
        /// Determine the time to wait before the weight would fit
        /// </summary>
        /// <returns></returns>
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
                    var result = entry.Timestamp + TimePeriod + _slidingWindowBuffer - DateTime.UtcNow;
                    if (result < TimeSpan.Zero)
                        return TimeSpan.Zero;
                    return result;
                }
            }

            throw new Exception("Request not possible to execute with current rate limit guard. " +
                        $" Request weight: {requestWeight}, Ratelimit: {Limit}");
        }
    }
}
