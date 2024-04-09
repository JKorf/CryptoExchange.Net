using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal class FixedWindowTracker : WindowTracker
    {
        /// <summary>
        /// Additional wait time to apply to account for time offset between server and client
        /// </summary>
        private static TimeSpan _fixedWindowBuffer = TimeSpan.FromMilliseconds(500);

        public FixedWindowTracker(int limit, TimeSpan period, int initialCount) : base(limit, period, initialCount)
        {
        }

        public override TimeSpan ProcessTopic(int weight)
        {
            // Remove requests no longer in time period from the history
            var checkTime = DateTime.UtcNow;
            RemoveBefore(checkTime.AddTicks(-(checkTime.Ticks % Timeperiod.Ticks)));

            if (_currentWeight + weight > Limit)
            {
                if (_currentWeight == 0)
                    throw new Exception("Request limit reached without any prior request. " +
                        $"This request can never execute with the current rate limiter. Request weight: {weight}, Ratelimit: {Limit}");

                // Wait until the next entry should be removed from the history
                return DetermineWaitTime();
            }

            return TimeSpan.Zero;
        }

        private TimeSpan DetermineWaitTime()
        {
            var checkTime = DateTime.UtcNow;
            var startCurrentWindow = checkTime.AddTicks(-(checkTime.Ticks % Timeperiod.Ticks));
            var wait = startCurrentWindow.Add(Timeperiod) - checkTime;
            return wait.Add(_fixedWindowBuffer);
        }
    }
}
