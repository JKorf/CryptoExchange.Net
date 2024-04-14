using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public class RetryAfterFilter : IGuardFilter
    {
        private readonly DateTime _retryAfter;
        public RetryAfterFilter(DateTime retryAfter)
        {
            _retryAfter = retryAfter;
        }

        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => DateTime.UtcNow >= _retryAfter;
    }
}
