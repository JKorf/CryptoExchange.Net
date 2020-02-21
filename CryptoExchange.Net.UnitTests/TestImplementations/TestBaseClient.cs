using System.Collections.Generic;
using System.Net.Http;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.UnitTests
{
    public class TestBaseClient: BaseClient
    {       
        public TestBaseClient(): base(new RestClientOptions("http://testurl.url"), null)
        {
        }

        public TestBaseClient(RestClientOptions exchangeOptions) : base(exchangeOptions, exchangeOptions.ApiCredentials == null ? null : new TestAuthProvider(exchangeOptions.ApiCredentials))
        {
        }

        public void Log(LogVerbosity verbosity, string data)
        {
            log.Write(verbosity, data);
        }

        public CallResult<T> Deserialize<T>(string data)
        {
            return Deserialize<T>(data, false);
        }

        public string FillParameters(string path, params string[] values)
        {
            return FillPathParameter(path, values);
        }
    }

    public class TestAuthProvider : AuthenticationProvider
    {
        public TestAuthProvider(ApiCredentials credentials) : base(credentials)
        {
        }

        public override Dictionary<string, string> AddAuthenticationToHeaders(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            return base.AddAuthenticationToHeaders(uri, method, parameters, signed);
        }

        public override Dictionary<string, object> AddAuthenticationToParameters(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            return base.AddAuthenticationToParameters(uri, method, parameters, signed);
        }

        public override string Sign(string toSign)
        {
            return toSign;
        }
    }
}
