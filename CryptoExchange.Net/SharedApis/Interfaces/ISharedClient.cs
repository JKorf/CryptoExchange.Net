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
        /// Whether or not API credentials have been configured for this client. Does not check the credentials are actually valid.
        /// </summary>
        bool Authenticated { get; }

        /// <summary>
        /// Options for each shared endpoint or subscription supported by the client.
        /// </summary>
        EndpointOptions[] AllOptions { get; }

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
        /// Set a default exchange parameter which will be statically set with each request. This can be used instead of passing it in an ExchangeParameters object with each request.<br />
        /// Default exchange parameters can still be overridden by passing the parameter in the ExchangeParameters of a request.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        void SetDefaultExchangeParameter(string name, object value);

        /// <summary>
        /// Reset previously set default exchange parameters for the exchange.
        /// </summary>
        void ResetDefaultExchangeParameters();
    }
}
