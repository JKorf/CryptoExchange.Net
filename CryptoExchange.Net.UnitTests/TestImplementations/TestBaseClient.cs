using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.UnitTests
{
    public class TestBaseClient: BaseClient
    {       
        public TestSubClient SubClient { get; }

        public TestBaseClient(): base("Test", new TestOptions())
        {
            SubClient = AddApiClient(new TestSubClient(new TestOptions(), new RestApiClientOptions()));
        }

        public TestBaseClient(ClientOptions exchangeOptions) : base("Test", exchangeOptions)
        {
        }

        public void Log(LogLevel verbosity, string data)
        {
            log.Write(verbosity, data);
        }
    }

    public class TestSubClient : RestApiClient
    {
        public TestSubClient(ClientOptions options, RestApiClientOptions apiOptions) : base(new Log(""), options, apiOptions)
        {
        }

        public CallResult<T> Deserialize<T>(string data) => Deserialize<T>(data, null, null);

        public override TimeSpan? GetTimeOffset() => null;
        public override TimeSyncInfo GetTimeSyncInfo() => null;
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials) => throw new NotImplementedException();
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();
    }

    public class TestAuthProvider : AuthenticationProvider
    {
        public TestAuthProvider(ApiCredentials credentials) : base(credentials)
        {
        }

        public override void AuthenticateRequest(RestApiClient apiClient, Uri uri, HttpMethod method, Dictionary<string, object> providedParameters, bool auth, ArrayParametersSerialization arraySerialization, HttpMethodParameterPosition parameterPosition, out SortedDictionary<string, object> uriParameters, out SortedDictionary<string, object> bodyParameters, out Dictionary<string, string> headers)
        {
            bodyParameters = new SortedDictionary<string, object>();
            uriParameters = new SortedDictionary<string, object>();
            headers = new Dictionary<string, string>();
        }

        public override string Sign(string toSign)
        {
            return toSign;
        }
    }
}
