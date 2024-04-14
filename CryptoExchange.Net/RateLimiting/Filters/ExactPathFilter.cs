using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public class ExactPathFilter : IGuardFilter
    {
        private readonly string _path;

        public ExactPathFilter(string path)
        {
            _path = path;
        }

        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => string.Equals(definition.Path, _path, StringComparison.OrdinalIgnoreCase);
    }
}
