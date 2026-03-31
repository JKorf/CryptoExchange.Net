using CryptoExchange.Net.Objects.Options;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestRestOptions : RestExchangeOptions<TestEnvironment, TestCredentials>
    {
        internal static TestRestOptions Default { get; set; } = new TestRestOptions()
        {
            Environment = TestEnvironment.Live,
            AutoTimestamp = true
        };

        public TestRestOptions()
        {
            Default?.Set(this);
        }

        public RestApiOptions ExchangeOptions { get; private set; } = new RestApiOptions();

        internal TestRestOptions Set(TestRestOptions targetOptions)
        {
            targetOptions = base.Set<TestRestOptions>(targetOptions);
            targetOptions.ExchangeOptions = ExchangeOptions.Set(targetOptions.ExchangeOptions);
            return targetOptions;
        }
    }
}
