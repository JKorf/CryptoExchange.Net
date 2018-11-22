using System;
using System.Collections.Generic;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.UnitTests
{
    public class TestImplementation: RestClient
    {
        public TestImplementation(): base(new ClientOptions(), null) { }

        public TestImplementation(ClientOptions exchangeOptions) : base(exchangeOptions, exchangeOptions.ApiCredentials == null ? null : new TestAuthProvider(exchangeOptions.ApiCredentials))
        {
        }

        public void SetApiCredentails(string key, string secret)
        {
            SetAuthenticationProvider(new TestAuthProvider(new ApiCredentials(key, secret)));
        }

        public CallResult<TestObject> TestCall()
        {
            return ExecuteRequest<TestObject>(new Uri("http://www.test.com")).Result;
        }
    }

    public class TestAuthProvider : AuthenticationProvider
    {
        public TestAuthProvider(ApiCredentials credentials) : base(credentials)
        {
        }

        public override Dictionary<string, string> AddAuthenticationToHeaders(string uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            return base.AddAuthenticationToHeaders(uri, method, parameters, signed);
        }

        public override Dictionary<string, object> AddAuthenticationToParameters(string uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            return base.AddAuthenticationToParameters(uri, method, parameters, signed);
        }

        public override string Sign(string toSign)
        {
            return toSign;
        }
    }

    public class TestObject
    {
        public int Id { get; set; }
        public List<string> Data { get; set; }
    }
}
