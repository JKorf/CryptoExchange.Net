using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Asset type
    /// </summary>
    public enum SharedAssetType
    {
        /// <summary>
        /// Unknown or unspecified asset type
        /// </summary>
        Unspecified,
        /// <summary>
        /// Cryptocurrency asset type
        /// </summary>
        Crypto,
        /// <summary>
        /// Fiat currency asset type
        /// </summary>
        Fiat,
        /// <summary>
        /// Traditional finance asset type
        /// </summary>
        TradFi
    }

    /// <summary>
    /// Asset sub type
    /// </summary>
    public enum SharedAssetSubType
    {
        // --- Crypto sub types ---
        /// <summary>
        /// Stable coin, can be for different fiat currencies
        /// </summary>
        StableCoin,

        // --- TradFi sub types ---
        /// <summary>
        /// Equity, can be stocks, ETFs, or indices
        /// </summary>
        Equity,
        /// <summary>
        /// Commodity, can be oil, gas, metals, etc.
        /// </summary>
        Commodity
    }
}
