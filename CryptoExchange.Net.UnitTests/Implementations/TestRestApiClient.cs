using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Testing.Implementations;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestRestApiClient : RestApiClient<TestEnvironment, TestAuthenticationProvider, TestCredentials>
    {
        protected override IRestMessageHandler MessageHandler { get; } = new TestRestMessageHandler();

        public TestRestApiClient(ILogger logger, HttpClient? httpClient, TestRestOptions options) 
            : base(logger, httpClient, options.Environment.RestClientAddress, options, options.ExchangeOptions)
        {
        }


        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null) =>
            baseAsset + quoteAsset;

        protected override TestAuthenticationProvider CreateAuthenticationProvider(TestCredentials credentials) =>
            new TestAuthenticationProvider(credentials);

        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(SerializerOptions.WithConverters(new TestSerializerContext()));

        internal void SetNextResponse(string data, HttpStatusCode code)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(data);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var response = new TestResponse(code, responseStream);
            var request = new TestRequest(response);

            var factory = new TestRequestFactory(request);
            RequestFactory = factory;
        }

        internal async Task<WebCallResult<T>> GetResponseAsync<T>(HttpMethod? httpMethod = null, ParameterCollection? collection = null)
        {
            var definition = new RequestDefinition("/path", httpMethod ?? HttpMethod.Get)
            {
                Weight = 0
            };
            return await SendAsync<T>(BaseAddress, definition, collection ?? new ParameterCollection(), default);
        }

        internal void SetParameterPosition(HttpMethod httpMethod, HttpMethodParameterPosition pos)
        {
            ParameterPositions[httpMethod] = pos;
        }
    }
}
