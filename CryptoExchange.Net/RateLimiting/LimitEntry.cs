using System;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// A rate limit entry
    /// </summary>
    public struct LimitEntry
    {
        /// <summary>
        /// Timestamp of the item
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Item weight
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="weight"></param>
        public LimitEntry(DateTime timestamp, int weight)
        {
            Timestamp = timestamp;
            Weight = weight;
        }
    }
}
