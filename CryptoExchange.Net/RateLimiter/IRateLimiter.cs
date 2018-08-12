using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.RateLimiter
{
    public interface IRateLimiter
    {
        CallResult<double> LimitRequest(string url, RateLimitingBehaviour limitBehaviour);
    }
}
