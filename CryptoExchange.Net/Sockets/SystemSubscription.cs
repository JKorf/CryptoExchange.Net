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
    public abstract class SystemSubscription : SubscriptionActor
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticated"></param>
        public SystemSubscription(ILogger logger, ISocketApiClient socketApiClient, bool authenticated = false) : base(logger, socketApiClient, authenticated)
        {
        }

        /// <inheritdoc />
        public override object? GetSubscribeRequest() => null;
        /// <inheritdoc />
        public override (bool, CallResult?) MessageMatchesSubscribeRequest(StreamMessage message) => throw new NotImplementedException();

        /// <inheritdoc />
        public override object? GetUnsubscribeRequest() => null;
        /// <inheritdoc />
        public override (bool, CallResult?) MessageMatchesUnsubscribeRequest(StreamMessage message) => throw new NotImplementedException();
    }
}
