using System;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Moq;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestSocketClient: BaseSocketClient
    {
        public TestSubSocketClient SubClient { get; }

        public TestSocketClient() : this(new TestOptions())
        {
        }

        public TestSocketClient(TestOptions exchangeOptions) : base("test", exchangeOptions)
        {
            SubClient = new TestSubSocketClient(exchangeOptions, exchangeOptions.SubOptions);
            SocketFactory = new Mock<IWebsocketFactory>().Object;
            Mock.Get(SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<Log>(), It.IsAny<string>())).Returns(new TestSocket());
        }

        public TestSocket CreateSocket()
        {
            Mock.Get(SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<Log>(), It.IsAny<string>())).Returns(new TestSocket());
            return (TestSocket)CreateSocket("123");
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

        protected internal override bool MessageMatchesHandler(SocketConnection s, JToken message, object request)
        {
            throw new NotImplementedException();
        }

        protected internal override bool MessageMatchesHandler(SocketConnection s, JToken message, string identifier)
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

    public class TestOptions: BaseSocketClientOptions
    {
        public ApiClientOptions SubOptions { get; set; } = new ApiClientOptions();
    }

    public class TestSubSocketClient : SocketApiClient
    {

        public TestSubSocketClient(BaseClientOptions options, ApiClientOptions apiOptions): base(options, apiOptions)
        {

        }

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new TestAuthProvider(credentials);
    }
}
