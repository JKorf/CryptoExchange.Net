using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestAuthenticationProvider : AuthenticationProvider<TestCredentials, TestCredentials>
    {
        public TestAuthenticationProvider(TestCredentials credentials) : base(credentials, credentials)
        {
        }

        public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
        {
            requestConfig.Headers ??= new Dictionary<string, string>();
            requestConfig.Headers["Authorization"] = Credential.Key;
        }
    }
}
