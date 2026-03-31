using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestRestClient : BaseRestClient<TestEnvironment, TestCredentials>
    {
        public TestRestApiClient ApiClient1 { get; set; }
        public TestRestApiClient ApiClient2 { get; set; }

        public TestRestClient(Action<TestRestOptions>? optionsDelegate = null)
            : this(null, null, Options.Create(ApplyOptionsDelegate(optionsDelegate)))
        {
        }

        public TestRestClient(HttpClient? httpClient, ILoggerFactory? loggerFactory, IOptions<TestRestOptions> options) : base(loggerFactory, "Test")
        {
            Initialize(options.Value);

            ApiClient1 = AddApiClient(new TestRestApiClient(_logger, httpClient, options.Value));
            ApiClient2 = AddApiClient(new TestRestApiClient(_logger, httpClient, options.Value));
        }
    }
}
