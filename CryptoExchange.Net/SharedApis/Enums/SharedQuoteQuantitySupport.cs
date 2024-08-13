using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Enums
{
    /// <summary>
    /// Quote asset order quantity support
    /// </summary>
    public enum SharedQuoteQuantitySupport
    {
        /// <summary>
        /// Market buy orders support specifying the quantity in the quote asset
        /// </summary>
        MarketBuy,
        /// <summary>
        /// Market buy orders are forced to have the quantity in the quote asset
        /// </summary>
        MarketBuyForced,
        /// <summary>
        /// No support for quote asset quantity
        /// </summary>
        None
    }
}
