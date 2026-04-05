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
        public TestSocketApiClient(ILogger logger, TestSocketOptions options)
            : base(logger, options.Environment.SocketClientAddress, options, options.ExchangeOptions)
        {
        }

        public TestSocketApiClient(ILogger logger, HttpClient httpClient, string baseAddress, TestSocketOptions options, SocketApiOptions apiOptions)
            : base(logger, baseAddress, options, apiOptions)
        {
        }

        public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType) => new TestSocketMessageHandler();
        protected internal override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(SerializerOptions.WithConverters(new TestSerializerContext()));

        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null) =>
            baseAsset + quoteAsset;

        protected override TestAuthenticationProvider CreateAuthenticationProvider(TestCredentials credentials) =>
            new TestAuthenticationProvider(credentials);

        public async Task<CallResult<UpdateSubscription>> SubscribeToUpdatesAsync<T>(Action<DataEvent<T>> handler, bool subQuery, CancellationToken ct)
        {
            return await base.SubscribeAsync(new TestSubscription<T>(_logger, handler, subQuery, false), ct);
        }
    }
}
