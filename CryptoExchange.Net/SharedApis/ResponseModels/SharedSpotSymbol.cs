namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Symbol info
    /// </summary>
    public record SharedSpotSymbol
    {
        /// <summary>
        /// Base asset of the symbol
        /// </summary>
        public string BaseAsset { get; set; }
        /// <summary>
        /// Quote asset of the symbol
        /// </summary>
        public string QuoteAsset { get; set; }
        /// <summary>
        /// The name of the symbol
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Minimal quantity of an order in the base asset
        /// </summary>
        public decimal? MinTradeQuantity { get; set; }
        /// <summary>
        /// Minimal notional value (quantity * price) of an order
        /// </summary>
        public decimal? MinNotionalValue { get; set; }
        /// <summary>
        /// Max quantity of an order in the base asset
        /// </summary>
        public decimal? MaxTradeQuantity { get; set; }
        /// <summary>
        /// Step by which the quantity should increase
        /// </summary>
        public decimal? QuantityStep { get; set; }
        /// <summary>
        /// Step by which the price should increase
        /// </summary>
        public decimal? PriceStep { get; set; }
        /// <summary>
        /// The max amount of decimal places for quantity
        /// </summary>
        public int? QuantityDecimals { get; set; }
        /// <summary>
        /// The max amount of decimal places for price
        /// </summary>
        public int? PriceDecimals { get; set; }
        /// <summary>
        /// Whether the symbol is currently available for trading
        /// </summary>
        public bool Trading { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSpotSymbol(string baseAsset, string quoteAsset, string symbol, bool trading)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            Name = symbol;
            Trading = trading;
        }
    }
}
