using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public class HostFilter : IGuardFilter
    {
        private readonly string _host;

        public HostFilter(string host)
        {
            _host = host;
        }

        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => host == _host;

    }
}
