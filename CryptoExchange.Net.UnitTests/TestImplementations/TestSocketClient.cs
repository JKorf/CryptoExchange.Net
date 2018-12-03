using System;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Moq;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestSocketClient: SocketClient
    {
        public Action OnReconnect { get; set; }

        public TestSocketClient() : this(new SocketClientOptions())
        {
        }

        public TestSocketClient(SocketClientOptions exchangeOptions) : base(exchangeOptions, exchangeOptions.ApiCredentials == null ? null : new TestAuthProvider(exchangeOptions.ApiCredentials))
        {
            SocketFactory = new Mock<IWebsocketFactory>().Object;
            Mock.Get(SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<Log>(), It.IsAny<string>())).Returns(new TestSocket());
        }

        public TestSocket CreateSocket()
        {
            return (TestSocket)CreateSocket(BaseAddress);
        }

        public CallResult<bool> ConnectSocketSub(SocketSubscription sub)
        {
            return ConnectSocket(sub).Result;
        }
        
        protected override bool SocketReconnect(SocketSubscription subscription, TimeSpan disconnectedTime)
        {
            OnReconnect?.Invoke();
            return true;
        }
    }
}
