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
        public override BaseQuery? GetSubQuery() => null;

        /// <inheritdoc />
        public override BaseQuery? GetUnsubQuery() => null;
    }

    public abstract class SystemSubscription<T> : SystemSubscription
    {
        public override Type ExpectedMessageType => typeof(T);
        public override Task<CallResult> DoHandleMessageAsync(DataEvent<BaseParsedMessage> message)
            => HandleMessageAsync(message.As((ParsedMessage<T>)message.Data));

        protected SystemSubscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }

        public abstract Task<CallResult> HandleMessageAsync(DataEvent<ParsedMessage<T>> message);
    }
}
