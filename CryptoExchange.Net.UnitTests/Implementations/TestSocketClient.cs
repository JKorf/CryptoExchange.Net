using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestSocketClient : BaseSocketClient<TestEnvironment, TestCredentials>
    {
        public TestSocketApiClient ApiClient1 { get; set; }
        public TestSocketApiClient ApiClient2 { get; set; }

        public TestSocketClient(Action<TestSocketOptions>? optionsDelegate = null)
            : this(null, Options.Create(ApplyOptionsDelegate(optionsDelegate)))
        {
        }

        public TestSocketClient(ILoggerFactory? loggerFactory, IOptions<TestSocketOptions> options) : base(loggerFactory, "Test")
        {
            Initialize(options.Value);

            ApiClient1 = AddApiClient(new TestSocketApiClient(_logger, options.Value));
            ApiClient2 = AddApiClient(new TestSocketApiClient(_logger, options.Value));
        }
    }
}
