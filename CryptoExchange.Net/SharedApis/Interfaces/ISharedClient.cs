using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    /// <summary>
    /// A shared/common client interface
    /// </summary>
    public interface ISharedClient
    {
        /// <summary>
        /// Name of the exchange
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// Which trading modes this client supports
        /// </summary>
        TradingMode[] SupportedTradingModes { get; }

        /// <summary>
        /// Set default exchange parameters. This can be used instead of passing in an ExchangeParameters object which each request.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        void SetDefaultExchangeParameter(string name, object value);

        /// <summary>
        /// Reset the default exchange parameters.
        /// </summary>
        void ResetDefaultExchangeParameters();
    }
}
