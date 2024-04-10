using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Limit guard
    /// </summary>
    public abstract class LimitGuard
    {
        /// <summary>
        /// The limit
        /// </summary>
        public int Limit { get; }
        /// <summary>
        /// The time period
        /// </summary>
        public TimeSpan TimeSpan { get; }

        private RateLimitWindowType _windowType;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        public LimitGuard(int limit, TimeSpan period)
        {
            Limit = limit;
            TimeSpan = period;
        }

        /// <summary>
        /// Create a new WindowTracker
        /// </summary>
        /// <returns></returns>
        protected IWindowTracker CreateTracker()
        {
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(Limit, TimeSpan) : new FixedWindowTracker(Limit, TimeSpan);
        }

        /// <summary>
        /// Set the window type
        /// </summary>
        /// <param name="type"></param>
        public void SetWindowType(RateLimitWindowType type) => _windowType = type;
    }
}
