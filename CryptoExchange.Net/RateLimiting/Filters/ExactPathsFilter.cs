using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    public class ExactPathsFilter : IGuardFilter
    {
        private readonly HashSet<string> _paths;

        public ExactPathsFilter(IEnumerable<string> paths)
        {
            _paths = new HashSet<string>(paths);
        }

        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => _paths.Contains(definition.Path);
    }
}
