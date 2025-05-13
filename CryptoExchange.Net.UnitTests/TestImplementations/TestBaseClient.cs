using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CryptoExchange.Net.UnitTests
{
    public class TestBaseClient: BaseClient
    {       
        public TestSubClient SubClient { get; }

        public TestBaseClient(): base(null, "Test")
        {
            var options = new TestClientOptions();
            _logger = NullLogger.Instance;
            Initialize(options);
            SubClient = AddApiClient(new TestSubClient(options, new RestApiOptions()));
        }

        public TestBaseClient(TestClientOptions exchangeOptions) : base(null, "Test")
        {
            _logger = NullLogger.Instance;
            Initialize(exchangeOptions);
            SubClient = AddApiClient(new TestSubClient(exchangeOptions, new RestApiOptions()));
        }

        public void Log(LogLevel verbosity, string data)
        {
            _logger.Log(verbosity, data);
        }
    }

    public class TestSubClient : RestApiClient
    {
        public TestSubClient(RestExchangeOptions<TestEnvironment> options, RestApiOptions apiOptions) : base(new TraceLogger(), null, "https://localhost:123", options, apiOptions)
        {
        }

        public CallResult<T> Deserialize<T>(string data)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var accessor = CreateAccessor();
            var valid = accessor.Read(stream, true).Result;
            if (!valid)
                return new CallResult<T>(new ServerError(data));
            
            var deserializeResult = accessor.Deserialize<T>();
            return deserializeResult;
        }

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode futuresType, DateTime? deliverDate = null) => $"{baseAsset.ToUpperInvariant()}{quoteAsset.ToUpperInvariant()}";
        public override TimeSpan? GetTimeOffset() => null;
        public override TimeSyncInfo GetTimeSyncInfo() => null;
        protected override IStreamMessageAccessor CreateAccessor() => new SystemTextJsonStreamMessageAccessor(new System.Text.Json.JsonSerializerOptions());
        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(new System.Text.Json.JsonSerializerOptions());
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials) => throw new NotImplementedException();
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();
    }

    public class TestAuthProvider : AuthenticationProvider
    {
        public TestAuthProvider(ApiCredentials credentials) : base(credentials)
        {
        }

        public override void AuthenticateRequest(RestApiClient apiClient, Uri uri, HttpMethod method, ref IDictionary<string, object> uriParams, ref IDictionary<string, object> bodyParams, ref Dictionary<string, string> headers, bool auth, ArrayParametersSerialization arraySerialization, HttpMethodParameterPosition parameterPosition, RequestBodyFormat bodyFormat)
        {
        }

        public string GetKey() => _credentials.Key;
        public string GetSecret() => _credentials.Secret;
    }
}
