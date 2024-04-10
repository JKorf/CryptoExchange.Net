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
        event Action<RateLimitEvent> RateLimitTriggered;

        /// <summary>
        /// Add a limit guard
        /// </summary>
        /// <param name="guard">Guard to add</param>
        /// <returns></returns>
        IRateLimitGate AddGuard(IRateLimitGuard guard);
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

        /// <summary>
        /// Process a request with an individual rate limit
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="key"></param>
        /// <param name="limit"></param>
        /// <param name="period"></param>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="requestWeight"></param>
        /// <param name="rateLimitingBehaviour"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CallResult> ProcessSingleAsync(ILogger logger, string key, int limit, TimeSpan period, RateLimitItemType type, Uri url, HttpMethod? method, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct);
    }
}
