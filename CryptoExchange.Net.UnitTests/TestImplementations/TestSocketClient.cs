﻿using System;
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
using CryptoExchange.Net.Testing.Implementations;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Options;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Net.WebSockets;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    internal class TestSocketClient: BaseSocketClient
    {
        public TestSubSocketClient SubClient { get; }

        /// <summary>
        /// Create a new instance of KucoinSocketClient
        /// </summary>
        /// <param name="optionsFunc">Configure the options to use for this client</param>
        public TestSocketClient(Action<TestSocketOptions> optionsDelegate = null)
            : this(Options.Create(ApplyOptionsDelegate(optionsDelegate)), null)
        {
        }

        public TestSocketClient(IOptions<TestSocketOptions> options, ILoggerFactory loggerFactory = null) : base(loggerFactory, "Test")
        {
            Initialize(options.Value);

            SubClient = AddApiClient(new TestSubSocketClient(options.Value, options.Value.SubOptions));
            SubClient.SocketFactory = new Mock<IWebsocketFactory>().Object;
            Mock.Get(SubClient.SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<ILogger>(), It.IsAny<WebSocketParameters>())).Returns(new TestSocket("https://test.com"));
        }

        public TestSocket CreateSocket()
        {
            Mock.Get(SubClient.SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<ILogger>(), It.IsAny<WebSocketParameters>())).Returns(new TestSocket("https://test.com"));
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

        /// <summary>
        /// ctor
        /// </summary>
        public TestSocketOptions()
        {
            Default?.Set(this);
        }

        public SocketApiOptions SubOptions { get; set; } = new SocketApiOptions();

        internal TestSocketOptions Set(TestSocketOptions targetOptions)
        {
            targetOptions = base.Set<TestSocketOptions>(targetOptions);
            targetOptions.SubOptions = SubOptions.Set(targetOptions.SubOptions);
            return targetOptions;
        }
    }

    public class TestSubSocketClient : SocketApiClient
    {
        private MessagePath _channelPath = MessagePath.Get().Property("channel");
        private MessagePath _actionPath = MessagePath.Get().Property("action");
        private MessagePath _topicPath = MessagePath.Get().Property("topic");

        public Subscription TestSubscription { get; private set; } = null;

        public TestSubSocketClient(TestSocketOptions options, SocketApiOptions apiOptions) : base(new TraceLogger(), options.Environment.TestAddress, options, apiOptions)
        {

        }

        protected internal override IByteMessageAccessor CreateAccessor(WebSocketMessageType type) => new SystemTextJsonByteMessageAccessor(new System.Text.Json.JsonSerializerOptions());
        protected internal override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(new System.Text.Json.JsonSerializerOptions());

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode futuresType, DateTime? deliverDate = null) => $"{baseAsset.ToUpperInvariant()}{quoteAsset.ToUpperInvariant()}";

        internal IWebsocket CreateSocketInternal(string address)
        {
            return CreateSocket(address);
        }

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new TestAuthProvider(credentials);

        public CallResult ConnectSocketSub(SocketConnection sub)
        {
            return ConnectSocketAsync(sub, default).Result;
        }

        public override string GetListenerIdentifier(IMessageAccessor message)
        {
            if (!message.IsValid)
            {
                return "topic";
            }

            var id = message.GetValue<string>(_channelPath);
            id ??= message.GetValue<string>(_topicPath);

            return message.GetValue<string>(_actionPath) + "-" + id;
        }

        public Task<CallResult<UpdateSubscription>> SubscribeToSomethingAsync(string channel, Action<DataEvent<string>> onUpdate, CancellationToken ct)
        {
            TestSubscription = new TestSubscriptionWithResponseCheck<string>(channel, onUpdate);
            return SubscribeAsync(TestSubscription, ct);
        }        
    }
}
