using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class SlidingWindowTracker : WindowTracker
    {
        public SlidingWindowTracker(int limit, TimeSpan period, int initialCount) : base (limit, period, initialCount)
        {
        }

        public override TimeSpan GetWaitTime(int weight)
        {
            // Remove requests no longer in time period from the history
            RemoveBefore(DateTime.UtcNow - Timeperiod);

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
                    return entry.Timestamp + Timeperiod - DateTime.UtcNow;
                }
            }

            throw new Exception("Request not possible to execute with current rate limit guard. " +
                        $" Request weight: {requestWeight}, Ratelimit: {Limit}");
        }
    }
}
