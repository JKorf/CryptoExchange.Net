using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using System.Security;

namespace CryptoExchange.Net.RateLimiting.Filters
{
    /// <summary>
    /// Filter requests based on whether they're authenticated or not
    /// </summary>
    public class AuthenticatedEndpointFilter : IGuardFilter
    {
        private readonly bool _authenticated;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="authenticated"></param>
        public AuthenticatedEndpointFilter(bool authenticated)
        {
            _authenticated = authenticated;
        }

        /// <inheritdoc />
        public bool Passes(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey)
            => definition.Authenticated == _authenticated;
    }
}
