using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using System.Collections.Generic;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    /// <summary>
    /// Filter requests based on whether the request path matches any specific path in a list
    /// </summary>
    public class ExactPathsFilter : IGuardFilter
    {
        private readonly HashSet<string> _paths;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="paths"></param>
        public ExactPathsFilter(IEnumerable<string> paths)
        {
            _paths = new HashSet<string>(paths);
        }

        /// <inheritdoc />
        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey)
            => _paths.Contains(definition.Path);
    }
}
