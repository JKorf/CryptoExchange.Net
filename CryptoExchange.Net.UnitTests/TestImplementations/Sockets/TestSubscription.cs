using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using System;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class TestSubscription<T> : Subscription
    {
        private readonly Action<DataEvent<T>> _handler;

        public TestSubscription(ILogger logger, Action<DataEvent<T>> handler) : base(logger, false)
        {
            _handler = handler;

            MessageMatcher = MessageMatcher.Create<T>("update-topic", DoHandleMessage);
        }

        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, T message)
        {
            _handler.Invoke(new DataEvent<T>("Test", message, receiveTime, originalData));
            return new CallResult(null);
        }

        protected override Query GetSubQuery(SocketConnection connection) => new TestQuery("sub", new object(), false, 1);
        protected override Query GetUnsubQuery(SocketConnection connection) => new TestQuery("unsub", new object(), false, 1);
    }
}
