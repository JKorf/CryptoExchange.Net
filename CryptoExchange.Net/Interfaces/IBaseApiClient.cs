using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Base api client
    /// </summary>
    public interface IBaseApiClient
    {
        /// <summary>
        /// Base address
        /// </summary>
        string BaseAddress { get; }

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
        /// Set the API credentials for this API client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="credentials"></param>
        void SetApiCredentials<T>(T credentials) where T : ApiCredentials;
    }
}