using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
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
        private bool _subQuery;

        public TestSubscription(ILogger logger, Action<DataEvent<T>> handler, bool subQuery, bool authenticated) : base(logger, authenticated, true)
        {
            _handler = handler;
            _subQuery = subQuery;

            MessageRouter = MessageRouter.CreateWithoutTopicFilter<T>("test", HandleUpdate);
        }

        protected override Query? GetSubQuery(SocketConnection connection)
        {
            if (!_subQuery)
                return null;

            return new TestQuery(new TestSocketMessage { Id = 1, Data = "Sub" }, false);
        }

        protected override Query? GetUnsubQuery(SocketConnection connection)
        {
            if (!_subQuery)
                return null;

            return new TestQuery(new TestSocketMessage { Id = 2, Data = "Unsub" }, false);
        }


        private CallResult? HandleUpdate(SocketConnection connection, DateTime time, string? originalData, T data)
        {
            _handler(new DataEvent<T>("Test", data, time, originalData));
            return CallResult.SuccessResult;
        }
    }
}
