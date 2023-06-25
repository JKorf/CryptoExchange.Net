using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestSocketClient: BaseSocketClient
    {
        public TestSubSocketClient SubClient { get; }

        public TestSocketClient(ILoggerFactory loggerFactory = null) : this((x) => { }, loggerFactory)
        {
        }

        /// <summary>
        /// Create a new instance of KucoinSocketClient
        /// </summary>
        /// <param name="optionsFunc">Configure the options to use for this client</param>
        public TestSocketClient(Action<TestSocketOptions> optionsFunc) : this(optionsFunc, null)
        {
        }

        public TestSocketClient(Action<TestSocketOptions> optionsFunc, ILoggerFactory loggerFactory = null) : base(loggerFactory, "Test")
        {
            var options = TestSocketOptions.Default.Copy<TestSocketOptions>();
            optionsFunc(options);
            Initialize(options);

            SubClient = AddApiClient(new TestSubSocketClient(options, options.SubOptions));
            SubClient.SocketFactory = new Mock<IWebsocketFactory>().Object;
            Mock.Get(SubClient.SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<ILogger>(), It.IsAny<WebSocketParameters>())).Returns(new TestSocket());
        }

        public TestSocket CreateSocket()
        {
            Mock.Get(SubClient.SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<ILogger>(), It.IsAny<WebSocketParameters>())).Returns(new TestSocket());
            return (TestSocket)SubClient.CreateSocketInternal("https://localhost:123/");
        }
                
    }

    public class TestEnvironment : TradeEnvironment
    {
        public string TestAddress { get; }

        public TestEnvironment(string name, string url) : base(name)
        {
            TestAddress = url;
        }
    }

    public class TestSocketOptions: SocketExchangeOptions<TestEnvironment>
    {
        public static TestSocketOptions Default = new TestSocketOptions
        {
            Environment = new TestEnvironment("Live", "https://test.test")
        };

        public SocketApiOptions SubOptions { get; set; } = new SocketApiOptions();
    }

    public class TestSubSocketClient : SocketApiClient
    {

        public TestSubSocketClient(TestSocketOptions options, SocketApiOptions apiOptions): base(new TraceLogger(), options.Environment.TestAddress, options, apiOptions)
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
