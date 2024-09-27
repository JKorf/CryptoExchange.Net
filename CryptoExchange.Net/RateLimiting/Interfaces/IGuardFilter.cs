using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.RateLimiting.Interfaces
{
    /// <summary>
    /// Filter requests based on specific condition
    /// </summary>
    public interface IGuardFilter
    {
        /// <summary>
        /// Whether a request or connection passes this filter
        /// </summary>
        /// <param name="type">The type of item</param>
        /// <param name="definition">The request definition</param>
        /// <param name="host">The host address</param>
        /// <param name="apiKey">The API key</param>
        /// <returns>True if passed</returns>
        bool Passes(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey);
    }
}
