namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Ticker data
    /// </summary>
    public class Ticker: BaseCommonObject
    {
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Price 24 hours ago
        /// </summary>
        public decimal? Price24H { get; set; }
        /// <summary>
        /// Last trade price
        /// </summary>
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// 24 hour low price
        /// </summary>
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// 24 hour high price
        /// </summary>
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// 24 hour volume
        /// </summary>
        public decimal? Volume { get; set; }
    }
}
