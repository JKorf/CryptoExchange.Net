using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// A system subscription
    /// </summary>
    public abstract class SystemSubscription : Subscription
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticated"></param>
        public SystemSubscription(ILogger logger, bool authenticated = false) : base(logger, authenticated)
        {
        }

        /// <inheritdoc />
        public override BaseQuery? GetSubQuery() => null;

        /// <inheritdoc />
        public override BaseQuery? GetUnsubQuery() => null;
    }
}
