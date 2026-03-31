using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestSubscription<T> : Subscription
    {
        private readonly Action<DataEvent<T>> _handler;

        public TestSubscription(ILogger logger, Action<DataEvent<T>> handler, bool authenticated) : base(logger, authenticated, true)
        {
            _handler = handler;

            MessageRouter = MessageRouter.CreateWithoutTopicFilter<T>("test", HandleUpdate);
        }

        protected override Query? GetSubQuery(SocketConnection connection) => null;
        protected override Query? GetUnsubQuery(SocketConnection connection) => null;


        private CallResult? HandleUpdate(SocketConnection connection, DateTime time, string? originalData, T data)
        {
            _handler(new DataEvent<T>("Test", data, time, originalData));
            return CallResult.SuccessResult;
        }
    }
}
