using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Rate limiter interface
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Limit the request if needed
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="limitBehaviour"></param>
        /// <param name="credits"></param>
        /// <returns></returns>
        CallResult<double> LimitRequest(RestClient client, string url, RateLimitingBehaviour limitBehaviour, int credits=1);
    }
}
