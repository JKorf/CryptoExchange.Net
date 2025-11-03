using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Mark/index price kline
    /// </summary>
    public record SharedFuturesKline : SharedSymbolModel
    {
        /// <summary>
        /// Open time
        /// </summary>
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// Close price
        /// </summary>
        public decimal ClosePrice { get; set; }
        /// <summary>
        /// High price
        /// </summary>
        public decimal HighPrice { get; set; }
        /// <summary>
        /// Low price
        /// </summary>
        public decimal LowPrice { get; set; }
        /// <summary>
        /// Open price
        /// </summary>
        public decimal OpenPrice { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFuturesKline(SharedSymbol? sharedSymbol, string symbol, DateTime openTime, decimal closePrice, decimal highPrice, decimal lowPrice, decimal openPrice) 
            : base(sharedSymbol, symbol)
        {
            OpenTime = openTime;
            ClosePrice = closePrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            OpenPrice = openPrice;
        }
    }
}
