using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    public abstract class LimitGuard
    {
        protected readonly int _limit;
        private int _initialCount;
        private readonly TimeSpan _timespan;
        private RateLimitWindowType _windowType;

        public LimitGuard(int limit, TimeSpan period)
        {
            _limit = limit;
            _timespan = period;
        }

        protected WindowTracker CreateTracker()
        {
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(_limit, _timespan, _initialCount) : new FixedWindowTracker(_limit, _timespan, _initialCount);
        }

        public void SetWindowType(RateLimitWindowType type) => _windowType = type;
        public void SetInitialCount(int initialCount) => _initialCount = initialCount;
    }
}
