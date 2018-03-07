using CryptoExchange.Net.RateLimiter;

namespace CryptoExchange.Net.Interfaces
{
    public interface IExchangeClient
    {
        IRequestFactory RequestFactory { get; set; }
        void AddRateLimiter(IRateLimiter limiter);
        void RemoveRateLimiters();
    }
}
