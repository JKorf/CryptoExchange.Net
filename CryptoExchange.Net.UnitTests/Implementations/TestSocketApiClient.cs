using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestSocketApiClient : SocketApiClient<TestEnvironment, TestAuthenticationProvider, TestCredentials>
    {
        public TestSocketApiClient(ILogger logger, HttpClient httpClient, string baseAddress, TestSocketOptions options, SocketApiOptions apiOptions)
            : base(logger, baseAddress, options, apiOptions)
        {
        }

        public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType) => throw new NotImplementedException();
        protected internal override IMessageSerializer CreateSerializer() => throw new NotImplementedException();

        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null) =>
            baseAsset + quoteAsset;

        protected override TestAuthenticationProvider CreateAuthenticationProvider(TestCredentials credentials) =>
            new TestAuthenticationProvider(credentials);
    }
}
