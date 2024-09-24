using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Mark/index price kline
    /// </summary>
    public record SharedFuturesKline
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
        public SharedFuturesKline(DateTime openTime, decimal closePrice, decimal highPrice, decimal lowPrice, decimal openPrice)
        {
            OpenTime = openTime;
            ClosePrice = closePrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            OpenPrice = openPrice;
        }
    }
}
