using System;

namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Kline data
    /// </summary>
    public class Kline: BaseCommonObject
    {
        /// <summary>
        /// Opening time of the kline
        /// </summary>
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// Price at the open time
        /// </summary>
        public decimal? OpenPrice { get; set; }
        /// <summary>
        /// Highest price of the kline
        /// </summary>
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// Lowest price of the kline
        /// </summary>
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// Close price of the kline
        /// </summary>
        public decimal? ClosePrice { get; set; }
        /// <summary>
        /// Volume of the kline
        /// </summary>
        public decimal? Volume { get; set; }
    }
}
