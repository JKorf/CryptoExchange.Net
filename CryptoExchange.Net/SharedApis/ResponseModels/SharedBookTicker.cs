namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Book ticker
    /// </summary>
    public record SharedBookTicker : SharedSymbolModel
    {
        /// <summary>
        /// Price of the best ask
        /// </summary>
        public decimal BestAskPrice { get; set; }
        /// <summary>
        /// Quantity of the best ask
        /// </summary>
        public decimal BestAskQuantity { get; set; }
        /// <summary>
        /// Price of the best bid
        /// </summary>
        public decimal BestBidPrice { get; set; }
        /// <summary>
        /// Quantity of the best bid
        /// </summary>
        public decimal BestBidQuantity { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedBookTicker(SharedSymbol? sharedSymbol, string symbol, decimal bestAskPrice, decimal bestAskQuantity, decimal bestBidPrice, decimal bestBidQuantity)
            : base(sharedSymbol, symbol)
        {
            BestAskPrice = bestAskPrice;
            BestAskQuantity = bestAskQuantity;
            BestBidPrice = bestBidPrice;
            BestBidQuantity = bestBidQuantity;
        }
    }
}
