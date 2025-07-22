using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class TestSubscriptionWithResponseCheck<T> : Subscription<SubResponse, UnsubResponse>
    {
        private readonly Action<DataEvent<T>> _handler;
        private readonly string _channel;

        public TestSubscriptionWithResponseCheck(string channel, Action<DataEvent<T>> handler) : base(Mock.Of<ILogger>(), false)
        {
            MessageMatcher = MessageMatcher.Create<T>(channel, DoHandleMessage);
            _handler = handler;
            _channel = channel;
        }

        public CallResult DoHandleMessage(SocketConnection connection, DataEvent<T> message)
        {
            _handler.Invoke(message);
            return new CallResult(null);
        }

        public override Query GetSubQuery(SocketConnection connection) => new TestChannelQuery(_channel, "subscribe", false, 1);
        public override Query GetUnsubQuery() => new TestChannelQuery(_channel, "unsubscribe", false, 1);
    }
}
