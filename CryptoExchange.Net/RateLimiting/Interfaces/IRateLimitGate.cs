using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting.Interfaces
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
        /// Add a rate limit guard
        /// </summary>
        /// <param name="guard">Guard to add</param>
        /// <returns></returns>
        IRateLimitGate AddGuard(IRateLimitGuard guard);

        /// <summary>
        /// Set a RetryAfter guard, can be used when a server rate limit is hit and a RetryAfter header is specified
        /// </summary>
        /// <param name="retryAfter">The time after which requests can be send again</param>
        /// <param name="type">RateLimitType</param>
        /// <returns></returns>
        Task SetRetryAfterGuardAsync(DateTime retryAfter, RateLimitItemType type = RateLimitItemType.Request);

        /// <summary>
        /// Returns the 'retry after' timestamp if set
        /// </summary>
        /// <returns></returns>
        Task<DateTime?> GetRetryAfterTime();

        /// <summary>
        /// Process a request. Enforces the configured rate limits. When a rate limit is hit will wait for the rate limit to pass if RateLimitingBehaviour is Wait, or return an error if it is set to Fail
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="itemId">Id of the item to check</param>
        /// <param name="type">The rate limit item type</param>
        /// <param name="definition">The request definition</param>
        /// <param name="baseAddress">The host address</param>
        /// <param name="apiKey">The API key</param>
        /// <param name="requestWeight">Request weight</param>
        /// <param name="behaviour">Behaviour when rate limit is hit</param>
        /// <param name="ct">Cancelation token</param>
        /// <returns>Error if RateLimitingBehaviour is Fail and rate limit is hit</returns>
        Task<CallResult> ProcessAsync(ILogger logger, int itemId, RateLimitItemType type, RequestDefinition definition, string baseAddress, string? apiKey, int requestWeight, RateLimitingBehaviour behaviour, CancellationToken ct);

        /// <summary>
        /// Enforces the rate limit as defined in the request definition. When a rate limit is hit will wait for the rate limit to pass if RateLimitingBehaviour is Wait, or return an error if it is set to Fail
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="itemId">Id of the item to check</param>
        /// <param name="guard">The guard</param>
        /// <param name="type">The rate limit item type</param>
        /// <param name="definition">The request definition</param>
        /// <param name="baseAddress">The host address</param>
        /// <param name="apiKey">The API key</param>
        /// <param name="behaviour">Behaviour when rate limit is hit</param>
        /// <param name="ct">Cancelation token</param>
        /// <returns>Error if RateLimitingBehaviour is Fail and rate limit is hit</returns>
        Task<CallResult> ProcessSingleAsync(ILogger logger, int itemId, IRateLimitGuard guard, RateLimitItemType type, RequestDefinition definition, string baseAddress, string? apiKey, RateLimitingBehaviour behaviour, CancellationToken ct);
    }
}
