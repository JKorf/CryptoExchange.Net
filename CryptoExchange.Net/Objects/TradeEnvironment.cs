namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Trade environment names
    /// </summary>
    public static class TradeEnvironmentNames
    {
        /// <summary>
        /// Live environment
        /// </summary>
        public const string Live = "live";
        /// <summary>
        /// Testnet environment
        /// </summary>
        public const string Testnet = "testnet";
    }

    /// <summary>
    /// Trade environment. Contains info about URL's to use to connect to the API. To swap environment select another environment for
    /// the echange's environment list or create a custom environment using either `[Exchange]Environment.CreateCustom()` or `[Exchange]Environment.[Environment]`, for example `KucoinEnvironment.TestNet` or `BinanceEnvironment.Live`
    /// </summary>
    public class TradeEnvironment
    {
        /// <summary>
        /// Name of the environment
        /// </summary>
        public string EnvironmentName { get; init; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        protected TradeEnvironment(string name)
        {
            EnvironmentName = name;
        }
    }
}
