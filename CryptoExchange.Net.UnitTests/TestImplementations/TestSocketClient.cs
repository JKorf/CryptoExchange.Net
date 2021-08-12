using System;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Moq;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestSocketClient: SocketClient
    {
        public TestSocketClient() : this(new SocketClientOptions("http://testurl.url"))
        {
        }

        public TestSocketClient(SocketClientOptions exchangeOptions) : base("test", exchangeOptions, exchangeOptions.ApiCredentials == null ? null : new TestAuthProvider(exchangeOptions.ApiCredentials))
        {
            SocketFactory = new Mock<IWebsocketFactory>().Object;
            Mock.Get(SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<Log>(), It.IsAny<string>())).Returns(new TestSocket());
        }

        public TestSocket CreateSocket()
        {
            Mock.Get(SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<Log>(), It.IsAny<string>())).Returns(new TestSocket());
            return (TestSocket)CreateSocket(BaseAddress);
        }

        public CallResult<bool> ConnectSocketSub(SocketConnection sub)
        {
            return ConnectSocketAsync(sub).Result;
        }

        protected internal override bool HandleQueryResponse<T>(SocketConnection s, object request, JToken data, out CallResult<T> callResult)
        {
            throw new NotImplementedException();
        }

        protected internal override bool HandleSubscriptionResponse(SocketConnection s, SocketSubscription subscription, object request, JToken message,
            out CallResult<object> callResult)
        {
            throw new NotImplementedException();
        }

        protected internal override bool MessageMatchesHandler(JToken message, object request)
        {
            throw new NotImplementedException();
        }

        protected internal override bool MessageMatchesHandler(JToken message, string identifier)
        {
            return true;
        }

        protected internal override Task<CallResult<bool>> AuthenticateSocketAsync(SocketConnection s)
        {
            throw new NotImplementedException();
        }

        protected internal override Task<bool> UnsubscribeAsync(SocketConnection connection, SocketSubscription s)
        {
            throw new NotImplementedException();
        }
    }
}
