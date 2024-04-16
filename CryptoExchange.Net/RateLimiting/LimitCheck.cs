using System;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Limit check
    /// </summary>
    public readonly struct LimitCheck
    {
        /// <summary>
        /// Is guard applicable
        /// </summary>
        public bool Applicable { get; }
        /// <summary>
        /// Delay needed
        /// </summary>
        public TimeSpan Delay { get; }
        /// <summary>
        /// Current counter
        /// </summary>
        public int Current { get; }
        /// <summary>
        /// Limit
        /// </summary>
        public int? Limit { get; }
        /// <summary>
        /// Time period
        /// </summary>
        public TimeSpan? Period { get; }

        private LimitCheck(bool applicable, TimeSpan delay, int limit, TimeSpan period, int current)
        {
            Applicable = applicable;
            Delay = delay;
            Limit = limit;
            Period = period;
            Current = current;
        }

        /// <summary>
        /// Not applicable
        /// </summary>
        public static LimitCheck NotApplicable { get; } = new LimitCheck(false, default, default, default, default);

        /// <summary>
        /// No wait needed
        /// </summary>
        public static LimitCheck NotNeeded { get; } = new LimitCheck(true, default, default, default, default);

        /// <summary>
        /// Wait needed
        /// </summary>
        /// <param name="delay">The delay needed</param>
        /// <param name="limit">Limit per period</param>
        /// <param name="period">Period the limit is for</param>
        /// <param name="current">Current counter</param>
        /// <returns></returns>
        public static LimitCheck Needed(TimeSpan delay, int limit, TimeSpan period, int current) => new(true, delay, limit, period, current);
    }
}
