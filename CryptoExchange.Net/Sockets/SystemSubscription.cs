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
        /// <param name="socketApiClient"></param>
        /// <param name="authenticated"></param>
        public SystemSubscription(ILogger logger, ISocketApiClient socketApiClient, bool authenticated = false) : base(logger, socketApiClient, authenticated)
        {
        }

        /// <inheritdoc />
        public override object? GetSubRequest() => null;
        /// <inheritdoc />
        public override (bool, CallResult?) MessageMatchesSubRequest(StreamMessage message) => throw new NotImplementedException();

        /// <inheritdoc />
        public override object? GetUnsubRequest() => null;
        /// <inheritdoc />
        public override (bool, CallResult?) MessageMatchesUnsubRequest(StreamMessage message) => throw new NotImplementedException();
    }
}
