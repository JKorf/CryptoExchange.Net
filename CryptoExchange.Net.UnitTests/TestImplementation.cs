using System;
using System.Collections.Generic;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.UnitTests
{
    public class TestImplementation: ExchangeClient
    {
        public TestImplementation(): base(new ExchangeOptions(), null) { }

        public TestImplementation(ExchangeOptions exchangeOptions) : base(exchangeOptions, exchangeOptions.ApiCredentials == null ? null : new TestAuthProvider(exchangeOptions.ApiCredentials))
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

        public override string AddAuthenticationToUriString(string uri, bool signed)
        {
            return uri;
        }

        public override IRequest AddAuthenticationToRequest(IRequest request, bool signed)
        {
            return request;
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
