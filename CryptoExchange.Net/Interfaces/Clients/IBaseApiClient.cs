using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.SharedApis;
using System;

namespace CryptoExchange.Net.Interfaces.Clients
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
        /// Whether or not API credentials have been configured for this client. Does not check the credentials are actually valid.
        /// </summary>
        bool Authenticated { get; }

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

        /// <summary>
        /// Set new options. Note that when using a proxy this should be provided in the options even when already set before or it will be reset.
        /// </summary>
        /// <typeparam name="T">Api credentials type</typeparam>
        /// <param name="options">Options to set</param>
        void SetOptions<T>(UpdateOptions<T> options) where T : ApiCredentials;
    }
}