using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Interfaces
{
    public interface IRateLimiter
    {
        CallResult<double> LimitRequest(string url, RateLimitingBehaviour limitBehaviour);
    }
}
