using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.UnitTests.TestImplementations.Sockets;
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
        private MessagePath _channelPath = MessagePath.Get().Property("channel");
        private MessagePath _topicPath = MessagePath.Get().Property("topic");

        public Subscription TestSubscription { get; private set; } = null;

        public TestSubSocketClient(TestSocketOptions options, SocketApiOptions apiOptions) : base(new TraceLogger(), options.Environment.TestAddress, options, apiOptions)
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

        public override string GetListenerIdentifier(IMessageAccessor message)
        {
            if (!message.IsJson)
            {
                return "topic";
            }

            var id = message.GetValue<string>(_channelPath);
            id ??= message.GetValue<string>(_topicPath);

            return id;
        }

        public Task<CallResult<UpdateSubscription>> SubscribeToSomethingAsync(string channel, Action<DataEvent<string>> onUpdate, CancellationToken ct)
        {
            TestSubscription = new TestSubscriptionWithResponseCheck<string>(channel, onUpdate);
            return SubscribeAsync(TestSubscription, ct);
        }        
    }
}
