using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using System;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    /// <summary>
    /// Filter requests based on whether the path starts with a specific string
    /// </summary>
    public class PathStartFilter : IGuardFilter
    {
        private readonly string _path;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="path"></param>
        public PathStartFilter(string path)
        {
            _path = path;
        }

        /// <inheritdoc />
        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey)
            => definition.Path.StartsWith(_path, StringComparison.OrdinalIgnoreCase);
    }
}
