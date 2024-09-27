using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    /// <summary>
    /// Filter requests based on whether the host address matches a specific address
    /// </summary>
    public class HostFilter : IGuardFilter
    {
        private readonly string _host;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="host"></param>
        public HostFilter(string host)
        {
            _host = host;
        }

        /// <inheritdoc />
        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey)
            => host == _host;

    }
}
