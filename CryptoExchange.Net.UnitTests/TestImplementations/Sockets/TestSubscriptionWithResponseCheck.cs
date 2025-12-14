using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class TestSubscriptionWithResponseCheck<T> : Subscription
    {
        private readonly Action<DataEvent<T>> _handler;
        private readonly string _channel;

        public TestSubscriptionWithResponseCheck(string channel, Action<DataEvent<T>> handler) : base(Mock.Of<ILogger>(), false)
        {
            MessageMatcher = MessageMatcher.Create<T>(channel, DoHandleMessage);
            _handler = handler;
            _channel = channel;
        }

        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, T message)
        {
            _handler.Invoke(new DataEvent<T>("Test", message, receiveTime, originalData));
            return new CallResult(null);
        }

        protected override Query GetSubQuery(SocketConnection connection) => new TestChannelQuery(_channel, "subscribe", false, 1);
        protected override Query GetUnsubQuery(SocketConnection connection) => new TestChannelQuery(_channel, "unsubscribe", false, 1);
    }
}
