using CryptoExchange.Net.Objects;
using System.Net.Http;
using System.Security;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Rate limit guard
    /// </summary>
    public interface IRateLimitGuard
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Check the rate limit
        /// </summary>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="signed"></param>
        /// <param name="apiKey"></param>
        /// <param name="requestWeight"></param>
        /// <returns></returns>
        LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight);
        /// <summary>
        /// Apply the rate limit token to the guard for tracking
        /// </summary>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="signed"></param>
        /// <param name="apiKey"></param>
        /// <param name="requestWeight"></param>
        /// <returns></returns>
        RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight);
    }
}
