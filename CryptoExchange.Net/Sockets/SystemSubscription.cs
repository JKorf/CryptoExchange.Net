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
        public override Query? GetSubQuery(SocketConnection connection) => null;

        /// <inheritdoc />
        public override Query? GetUnsubQuery() => null;
    }

    /// <inheritdoc />
    public abstract class SystemSubscription<T> : SystemSubscription
    {
        /// <inheritdoc />
        public override Type GetMessageType(IMessageAccessor message) => typeof(T);

        /// <inheritdoc />
        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
            => HandleMessage(connection, message.As((T)message.Data));

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticated"></param>
        protected SystemSubscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }

        /// <summary>
        /// Handle an update message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract CallResult HandleMessage(SocketConnection connection, DataEvent<T> message);
    }
}
