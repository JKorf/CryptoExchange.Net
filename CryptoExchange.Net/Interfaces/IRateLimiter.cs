using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Interfaces
{
    public interface IRateLimiter
    {
        CallResult<double> LimitRequest(RestClient client, string url, RateLimitingBehaviour limitBehaviour);
    }
}
