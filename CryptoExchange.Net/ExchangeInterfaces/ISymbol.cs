using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common symbol
    /// </summary>
    public interface ICommonSymbol
    {
        /// <summary>
        /// base asset name e.g. ETH
        /// </summary>
        public string CommonBaseAsset { get; }
        /// <summary>
        /// quote asset name e.g. BTC
        /// </summary>
        public string CommonQuoteAsset { get; }

        /// <summary>
        /// Symbol name
        /// </summary>
        public string CommonName { get; }
        /// <summary>
        /// Minimum trade size
        /// </summary>
        public decimal CommonMinimumTradeSize { get; }
        /// <summary>
        ///  price tick, e.g. for BTCUSDT pair at most exchanges it would be 0.01
        /// </summary>
        public decimal? CommonPriceTick { get; }
    }
}
