using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.SharedApis
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
        /// Format a base and quote asset to an exchange accepted symbol 
        /// </summary>
        /// <param name="baseAsset">The base asset</param>
        /// <param name="quoteAsset">The quote asset</param>
        /// <param name="tradingMode">The trading mode</param>
        /// <param name="deliverDate">The deliver date for a delivery futures symbol</param>
        /// <returns></returns>
        string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null);

        /// <summary>
        /// Set a default exchange parameter. This can be used instead of passing in an ExchangeParameters object which each request.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        void SetDefaultExchangeParameter(string name, object value);

        /// <summary>
        /// Reset the default exchange parameters, resets parameters for all exchanges
        /// </summary>
        void ResetDefaultExchangeParameters();
    }
}
