using System;

namespace CryptoExchange.Net.RateLimiting.Trackers
{
    internal static class WindowTrackerHelpers
    {
        private static readonly TimeSpan _minimumSafetyMargin = TimeSpan.FromMilliseconds(10);
        private static readonly TimeSpan _maximumSafetyMargin = TimeSpan.FromMilliseconds(250);

        public static TimeSpan GetDefaultSafetyMargin(TimeSpan period)
        {
            var margin = TimeSpan.FromTicks(period.Ticks / 20);
            if (margin < _minimumSafetyMargin)
                return _minimumSafetyMargin;
            if (margin > _maximumSafetyMargin)
                return _maximumSafetyMargin;
            return margin;
        }
    }
}
