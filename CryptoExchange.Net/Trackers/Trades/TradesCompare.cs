namespace CryptoExchange.Net.Trackers.Trades
{
    /// <summary>
    /// Trades statistics comparison
    /// </summary>
    public record TradesCompare
    {
        /// <summary>
        /// Number of trades
        /// </summary>
        public CompareValue TradeCountDif { get; set; } = new CompareValue(null, null);
        /// <summary>
        /// Average trade price
        /// </summary>
        public CompareValue? AveragePriceDif { get; set; }
        /// <summary>
        /// Volume weighted average trade price
        /// </summary>
        public CompareValue? VolumeWeightedAveragePriceDif { get; set; }
        /// <summary>
        /// Volume of the trades
        /// </summary>
        public CompareValue VolumeDif { get; set; } = new CompareValue(null, null);
        /// <summary>
        /// Volume of the trades in quote asset
        /// </summary>
        public CompareValue QuoteVolumeDif { get; set; } = new CompareValue(null, null);
        /// <summary>
        /// The volume weighted Buy/Sell ratio. A 0.7 ratio means 70% of the trade volume was a buy.
        /// </summary>
        public CompareValue? BuySellRatioDif { get; set; }
    }
}
