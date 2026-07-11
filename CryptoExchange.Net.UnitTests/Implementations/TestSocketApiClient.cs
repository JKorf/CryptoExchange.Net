using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestSocketApiClient : SocketApiClient<TestEnvironment, TestAuthenticationProvider, TestCredentials>
    {
        public TestSocketApiClient(ILoggerFactory? loggerFactory, TestSocketOptions options)
            : base(loggerFactory, "Test", options.Environment.SocketClientAddress, options, options.ExchangeOptions)
        {
        }

        public TestSocketApiClient(ILoggerFactory? loggerFactory, HttpClient httpClient, string baseAddress, TestSocketOptions options, SocketApiOptions apiOptions)
            : base(loggerFactory, "Test", baseAddress, options, apiOptions)
        {
        }

        public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType) => new TestSocketMessageHandler();
        protected internal override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(SerializerOptions.WithConverters(new TestSerializerContext()));

        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null) =>
            baseAsset + quoteAsset;

        protected override TestAuthenticationProvider CreateAuthenticationProvider(TestCredentials credentials) =>
            new TestAuthenticationProvider(credentials);

        public async Task<WebSocketResult<UpdateSubscription>> SubscribeToUpdatesAsync<T>(Action<DataEvent<T>> handler, bool subQuery, CancellationToken ct, int individualSubscriptionCount = 1)
        {
            var subscription = new TestSubscription<T>(_logger, handler, subQuery, false)
            {
                IndividualSubscriptionCount = individualSubscriptionCount
            };
            return await base.SubscribeAsync(subscription, ct);
        }
    }
}
