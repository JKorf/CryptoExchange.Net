using System;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Limit check
    /// </summary>
    public struct LimitCheck
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
        /// Current count
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
        /// <param name="delay"></param>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static LimitCheck Needed(TimeSpan delay, int limit, TimeSpan period, int current) => new LimitCheck(true, delay, limit, period, current);
    }
}
