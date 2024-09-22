using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.RateLimiting.Interfaces
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
        /// Check whether a request can pass this rate limit guard
        /// </summary>
        /// <param name="type">The rate limit item type</param>
        /// <param name="definition">The request definition</param>
        /// <param name="host">The host address</param>
        /// <param name="apiKey">The API key</param>
        /// <param name="requestWeight">The request weight</param>
        /// <returns></returns>
        LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight);

        /// <summary>
        /// Apply the request to this guard with the specified weight
        /// </summary>
        /// <param name="type">The rate limit item type</param>
        /// <param name="definition">The request definition</param>
        /// <param name="host">The host address</param>
        /// <param name="apiKey">The API key</param>
        /// <param name="requestWeight">The request weight</param>
        /// <returns></returns>
        RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight);
    }
}
