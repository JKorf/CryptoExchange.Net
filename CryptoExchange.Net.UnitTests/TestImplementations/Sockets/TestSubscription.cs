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

        public override HashSet<string> ListenerIdentifiers { get; set; } = new HashSet<string> { "update-topic" };

        public TestSubscription(ILogger logger, Action<DataEvent<T>> handler) : base(logger, false)
        {
            _handler = handler;
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var data = (T)message.Data;
            _handler.Invoke(message.As(data));
            return new CallResult(null);
        }

        public override Type GetMessageType(IMessageAccessor message) => typeof(T);
        public override Query GetSubQuery(SocketConnection connection) => new TestQuery("sub", new object(), false, 1);
        public override Query GetUnsubQuery() => new TestQuery("unsub", new object(), false, 1);
    }
}
