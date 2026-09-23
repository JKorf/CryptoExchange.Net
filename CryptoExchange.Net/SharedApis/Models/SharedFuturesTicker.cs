using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures ticker info
    /// </summary>
    [DebuggerDisplay("{Symbol,nq} High: {HighPrice}, Low: {LowPrice}, Last: {LastPrice}, Change: {ChangePercentage}%")]
    public record SharedFuturesTicker: SharedTicker
    {
        /// <summary>
        /// Current mark price
        /// </summary>
        public decimal? MarkPrice { get; set; }
        /// <summary>
        /// Current index price
        /// </summary>
        public decimal? IndexPrice { get; set; }
        /// <summary>
        /// Current funding rate
        /// </summary>
        public decimal? FundingRate { get; set; }
        /// <summary>
        /// Next funding time
        /// </summary>
        public DateTime? NextFundingTime { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFuturesTicker(
            SharedSymbol? sharedSymbol, 
            string symbol, 
            decimal? lastPrice, 
            decimal? highPrice,
            decimal? lowPrice, 
            SharedOrderQuantity volumes, 
            decimal? changePercentage)
            :base(sharedSymbol, symbol, lastPrice, highPrice, lowPrice, volumes, changePercentage)
        {
        }
    }
}
