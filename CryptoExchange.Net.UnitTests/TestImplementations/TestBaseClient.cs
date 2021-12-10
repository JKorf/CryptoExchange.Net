using System;
using System.Collections.Generic;
using System.Net.Http;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.UnitTests
{
    public class TestBaseClient: BaseClient
    {       
        public TestBaseClient(): base("Test", new BaseClientOptions())
        {
        }

        public TestBaseClient(BaseRestClientOptions exchangeOptions) : base("Test", exchangeOptions)
        {
        }

        public void Log(LogLevel verbosity, string data)
        {
            log.Write(verbosity, data);
        }

        public CallResult<T> Deserialize<T>(string data)
        {
            return Deserialize<T>(data, null, null);
        }
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
