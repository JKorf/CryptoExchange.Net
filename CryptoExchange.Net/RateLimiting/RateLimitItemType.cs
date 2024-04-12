using System;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Rate limit item type
    /// </summary>
    [Flags]
    public enum RateLimitItemType
    {
        /// <summary>
        /// A new connection
        /// </summary>
        Connection = 1,
        /// <summary>
        /// A request
        /// </summary>
        Request = 2
    }
}
