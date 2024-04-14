using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public class LimitItemTypeFilter : IGuardFilter
    {
        public readonly RateLimitItemType _type;

        public LimitItemTypeFilter(RateLimitItemType type)
        {
            _type = type;
        }

        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => type == _type;
    }
}
