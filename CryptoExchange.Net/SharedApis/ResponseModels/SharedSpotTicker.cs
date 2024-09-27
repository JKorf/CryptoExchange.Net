namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Ticker info
    /// </summary>
    public record SharedSpotTicker
    {
        /// <summary>
        /// Symbol name 
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Last trade price
        /// </summary>
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// Highest price in last 24h
        /// </summary>
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// Lowest price in last 24h
        /// </summary>
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// Trade volume in base asset in the last 24h
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// Change percentage in the last 24h
        /// </summary>
        public decimal? ChangePercentage { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSpotTicker(string symbol, decimal? lastPrice, decimal? highPrice, decimal? lowPrice, decimal volume, decimal? changePercentage)
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
