using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
        }

        /// <inheritdoc />
        public override BaseQuery? GetSubQuery(SocketConnection connection) => null;

        /// <inheritdoc />
        public override BaseQuery? GetUnsubQuery() => null;
    }

    public abstract class SystemSubscription<T> : SystemSubscription
    {
        public override Func<string, Type> ExpectedTypeDelegate => (x) => typeof(T);
        public override Task<CallResult> DoHandleMessageAsync(SocketConnection connection, DataEvent<BaseParsedMessage> message)
            => HandleMessageAsync(connection, message.As((ParsedMessage<T>)message.Data));

        protected SystemSubscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }

        public abstract Task<CallResult> HandleMessageAsync(SocketConnection connection, DataEvent<ParsedMessage<T>> message);
    }
}
