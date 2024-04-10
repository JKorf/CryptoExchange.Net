using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Access limiter
    /// </summary>
    public interface IAccessLimiter
    {
        /// <summary>
        /// Event when a rate limit is triggered
        /// </summary>
        event Action<RateLimitEvent> RateLimitTriggered;
    }
}
