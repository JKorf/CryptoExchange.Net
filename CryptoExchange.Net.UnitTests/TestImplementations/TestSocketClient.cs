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
            SubClient = AddApiClient(new TestSubSocketClient(exchangeOptions, exchangeOptions.SubOptions));
            SubClient.SocketFactory = new Mock<IWebsocketFactory>().Object;
            Mock.Get(SubClient.SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<Log>(), It.IsAny<WebSocketParameters>())).Returns(new TestSocket());
        }

        public TestSocket CreateSocket()
        {
            Mock.Get(SubClient.SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<Log>(), It.IsAny<WebSocketParameters>())).Returns(new TestSocket());
            return (TestSocket)SubClient.CreateSocketInternal("https://localhost:123/");
        }
                
    }

    public class TestOptions: ClientOptions
    {
        public SocketApiClientOptions SubOptions { get; set; } = new SocketApiClientOptions();
    }

    public class TestSubSocketClient : SocketApiClient
    {

        public TestSubSocketClient(ClientOptions options, SocketApiClientOptions apiOptions): base(new Log(""), options, apiOptions)
        {

        }

        internal IWebsocket CreateSocketInternal(string address)
        {
            return CreateSocket(address);
        }

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new TestAuthProvider(credentials);

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
}
