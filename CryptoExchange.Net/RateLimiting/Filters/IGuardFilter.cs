using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public interface IGuardFilter
    {
        bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey);
    }
}
