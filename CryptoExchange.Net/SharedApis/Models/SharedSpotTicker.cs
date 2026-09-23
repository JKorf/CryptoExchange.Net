using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Ticker info
    /// </summary>
    [DebuggerDisplay("{Symbol,nq} High: {HighPrice}, Low: {LowPrice}, Last: {LastPrice}, Change: {ChangePercentage}%")]
    public record SharedSpotTicker : SharedTicker
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SharedSpotTicker(
            SharedSymbol? sharedSymbol,
            string symbol,
            decimal? lastPrice,
            decimal? highPrice,
            decimal? lowPrice,
            SharedOrderQuantity volumes,
            decimal? changePercentage)
            : base(sharedSymbol, symbol, lastPrice, highPrice, lowPrice, volumes, changePercentage)
        {
        }
    }
}
