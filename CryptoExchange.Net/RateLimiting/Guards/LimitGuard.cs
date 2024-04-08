using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    public abstract class LimitGuard
    {
        private readonly int _limit;
        private readonly TimeSpan _timespan;
        private RateLimitWindowType _windowType;

        public LimitGuard(int limit, TimeSpan period)
        {
            _limit = limit;
            _timespan = period;
        }

        protected WindowTracker CreateTracker()
        {
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(_limit, _timespan) : new FixedWindowTracker(_limit, _timespan);
        }

        public void SetWindowType(RateLimitWindowType type) => _windowType = type;
    }
}
