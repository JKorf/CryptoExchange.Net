using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Rate limit gate
    /// </summary>
    public interface IRateLimitGate
    {
        /// <summary>
        /// Event when the rate limit is triggered
        /// </summary>
        event Action<string, HttpMethod?, TimeSpan> RateLimitTriggered;

        /// <summary>
        /// Add a limit guard
        /// </summary>
        /// <param name="guard">Guard to add</param>
        /// <param name="initialCount">Initial count</param>
        /// <returns></returns>
        IRateLimitGate AddGuard(IRateLimitGuard guard, int initialCount = 0);
        /// <summary>
        /// Set a specific window type
        /// </summary>
        /// <param name="type">Type of window</param>
        /// <returns></returns>
        IRateLimitGate WithWindowType(RateLimitWindowType type);

        /// <summary>
        /// Set a RetryAfter guard, can be used when a server rate limit is hit and a RetryAfter header is specified
        /// </summary>
        /// <param name="retryAfter"></param>
        /// <returns></returns>
        Task SetRetryAfterGuardAsync(DateTime retryAfter);
        /// <summary>
        /// Process a request
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="signed"></param>
        /// <param name="apiKey"></param>
        /// <param name="requestWeight"></param>
        /// <param name="behaviour"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CallResult> ProcessAsync(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour behaviour, CancellationToken ct);
    }
}
