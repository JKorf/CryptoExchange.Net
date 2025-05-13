namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Ticker info
    /// </summary>
    public record SharedSpotTicker: SharedSymbolModel
    {
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
        /// Trade volume in quote asset in the last 24h
        /// </summary>
        public decimal? QuoteVolume { get; set; }
        /// <summary>
        /// Change percentage in the last 24h
        /// </summary>
        public decimal? ChangePercentage { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSpotTicker(SharedSymbol? sharedSymbol, string symbol, decimal? lastPrice, decimal? highPrice, decimal? lowPrice, decimal volume, decimal? changePercentage)
            : base(sharedSymbol, symbol)
        {
            LastPrice = lastPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volume = volume;
            ChangePercentage = changePercentage;
        }
    }
}
