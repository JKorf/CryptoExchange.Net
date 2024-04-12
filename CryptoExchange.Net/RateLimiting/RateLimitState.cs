using System;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Limit state
    /// </summary>
    public struct RateLimitState
    {
        /// <summary>
        /// Limit
        /// </summary>
        public int Limit { get; }
        /// <summary>
        /// Period 
        /// </summary>
        public TimeSpan Period { get; }
        /// <summary>
        /// Current count
        /// </summary>
        public int Current { get; }
        /// <summary>
        /// Whether the limit is applied
        /// </summary>
        public bool IsApplied { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="applied"></param>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        /// <param name="current"></param>
        public RateLimitState(bool applied, int limit, TimeSpan period, int current)
        {
            IsApplied = applied;
            Limit = limit;
            Period = period;
            Current = current;
        }

        /// <summary>
        /// Not applied result
        /// </summary>
        public static RateLimitState NotApplied { get; } = new RateLimitState(false, default, default, default);
        /// <summary>
        /// Applied result
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static RateLimitState Applied(int limit, TimeSpan period, int current) => new RateLimitState(true, limit, period, current);
    }
}
