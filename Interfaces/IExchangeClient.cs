using System;
using System.Collections.Generic;
using System.Text;
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
