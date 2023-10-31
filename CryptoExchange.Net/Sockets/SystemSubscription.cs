using CryptoExchange.Net.Converters;
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
        public SystemSubscription(ILogger logger, bool authenticated = false) : base(logger, authenticated)
        {
        }

        /// <inheritdoc />
        public override object? GetSubRequest() => null;
        /// <inheritdoc />
        public override (bool, CallResult?) MessageMatchesSubRequest(ParsedMessage message) => throw new NotImplementedException();

        /// <inheritdoc />
        public override object? GetUnsubRequest() => null;
        /// <inheritdoc />
        public override (bool, CallResult?) MessageMatchesUnsubRequest(ParsedMessage message) => throw new NotImplementedException();
    }
}
