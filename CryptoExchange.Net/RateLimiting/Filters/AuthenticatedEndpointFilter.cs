using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public class AuthenticatedEndpointFilter : IGuardFilter
    {
        private readonly bool _authenticated;

        public AuthenticatedEndpointFilter(bool authenticated)
        {
            _authenticated = authenticated;
        }

        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => definition.Authenticated == _authenticated;
    }
}
