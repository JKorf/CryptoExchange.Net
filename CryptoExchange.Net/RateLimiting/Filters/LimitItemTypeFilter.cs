using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    /// <summary>
    /// Filter requests based on whether it's a connection or a request
    /// </summary>
    public class LimitItemTypeFilter : IGuardFilter
    {
        private readonly RateLimitItemType _type;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        public LimitItemTypeFilter(RateLimitItemType type)
        {
            _type = type;
        }

        /// <inheritdoc />
        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey)
            => type == _type;
    }
}
