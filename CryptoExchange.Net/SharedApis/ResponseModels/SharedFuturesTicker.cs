using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures ticker info
    /// </summary>
    public record SharedFuturesTicker
    {
        /// <summary>
        /// The symbol
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Last trade price
        /// </summary>
        public decimal LastPrice { get; set; }
        /// <summary>
        /// High price in the last 24h
        /// </summary>
        public decimal HighPrice { get; set; }
        /// <summary>
        /// Low price in the last 24h
        /// </summary>
        public decimal LowPrice { get; set; }
        /// <summary>
        /// The volume in the last 24h
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// Change percentage in the last 24h
        /// </summary>
        public decimal? ChangePercentage { get; set; }
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
        public SharedFuturesTicker(string symbol, decimal lastPrice, decimal highPrice, decimal lowPrice, decimal volume, decimal? changePercentage)
        {
            Symbol = symbol;
            LastPrice = lastPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volume = volume;
            ChangePercentage = changePercentage;
        }
    }
}
