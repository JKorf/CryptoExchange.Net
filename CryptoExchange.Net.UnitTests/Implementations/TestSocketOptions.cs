using CryptoExchange.Net.Objects.Options;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal class TestSocketOptions : SocketExchangeOptions<TestEnvironment, TestCredentials>
    {
        internal static TestSocketOptions Default { get; set; } = new TestSocketOptions()
        {
            Environment = TestEnvironment.Live,
            AutoTimestamp = true
        };

        public TestSocketOptions()
        {
            Default?.Set(this);
        }

        public SocketApiOptions ExchangeOptions { get; private set; } = new SocketApiOptions();

        internal TestSocketOptions Set(TestSocketOptions targetOptions)
        {
            targetOptions = base.Set<TestSocketOptions>(targetOptions);
            targetOptions.ExchangeOptions = ExchangeOptions.Set(targetOptions.ExchangeOptions);
            return targetOptions;
        }
    }
}
