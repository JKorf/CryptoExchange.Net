using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;

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
        public SystemSubscription(ILogger logger, bool authenticated = false) : base(logger, authenticated, false)
        {
            Confirmed = true;
        }

        /// <inheritdoc />
        protected override Query? GetSubQuery(SocketConnection connection) => null;

        /// <inheritdoc />
        protected override Query? GetUnsubQuery(SocketConnection connection) => null;
    }
}
