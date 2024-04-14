using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public class PathStartFilter : IGuardFilter
    {
        private readonly string _path;

        public PathStartFilter(string path)
        {
            _path = path;
        }

        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => _path.StartsWith(_path, StringComparison.OrdinalIgnoreCase);
    }
}
