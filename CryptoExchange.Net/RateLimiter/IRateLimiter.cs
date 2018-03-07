namespace CryptoExchange.Net.RateLimiter
{
    public interface IRateLimiter
    {
        double LimitRequest(string url);
    }
}
