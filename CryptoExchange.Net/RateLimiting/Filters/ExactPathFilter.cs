using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using System;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    /// <summary>
    /// Filter requests based on whether the request path matches a specific path
    /// </summary>
    public class ExactPathFilter : IGuardFilter
    {
        private readonly string _path;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="path"></param>
        public ExactPathFilter(string path)
        {
            _path = path;
        }

        /// <inheritdoc />
        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey)
            => string.Equals(definition.Path, _path, StringComparison.OrdinalIgnoreCase);
    }
}
