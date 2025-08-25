using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class TestSubscription<T> : Subscription<object, object>
    {
        private readonly Action<DataEvent<T>> _handler;

        public TestSubscription(ILogger logger, Action<DataEvent<T>> handler) : base(logger, false)
        {
            _handler = handler;

            MessageMatcher = MessageMatcher.Create<T>("update-topic", DoHandleMessage);
        }

        public CallResult DoHandleMessage(SocketConnection connection, DataEvent<T> message)
        {
            _handler.Invoke(message);
            return new CallResult(null);
        }

        protected override Query GetSubQuery(SocketConnection connection) => new TestQuery("sub", new object(), false, 1);
        protected override Query GetUnsubQuery(SocketConnection connection) => new TestQuery("unsub", new object(), false, 1);
    }
}
