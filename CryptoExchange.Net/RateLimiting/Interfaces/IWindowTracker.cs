using System;

namespace CryptoExchange.Net.RateLimiting.Interfaces
{
    /// <summary>
    /// Rate limit window tracker
    /// </summary>
    public interface IWindowTracker
    {
        /// <summary>
        /// Time period the limit is for
        /// </summary>
        TimeSpan TimePeriod { get; }
        /// <summary>
        /// The limit in the time period
        /// </summary>
        int Limit { get; }
        /// <summary>
        /// The current count within the time period
        /// </summary>
        int Current { get; }
        /// <summary>
        /// Get the time to wait to fit the weight
        /// </summary>
        TimeSpan GetWaitTime(int weight, double allowedRateRatio);
        /// <summary>
        /// Register the weight in this window
        /// </summary>
        /// <param name="weight">Request weight</param>
        void ApplyWeight(int weight);
        /// <summary>
        /// Reset the limit counter for this tracker
        /// </summary>
        void Reset(int? amount);
    }
}
