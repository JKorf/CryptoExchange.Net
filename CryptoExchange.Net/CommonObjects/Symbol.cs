namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Symbol data
    /// </summary>
    public class Symbol: BaseCommonObject
    {
        /// <summary>
        /// Name of the symbol
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Minimal quantity of an order
        /// </summary>
        public decimal? MinTradeQuantity { get; set; }
        /// <summary>
        /// Step with which the quantity should increase
        /// </summary>
        public decimal? QuantityStep { get; set; } 
        /// <summary>
        /// step with which the price should increase
        /// </summary>
        public decimal? PriceStep { get; set; } 
        /// <summary>
        /// The max amount of decimals for quantity
        /// </summary>
        public int? QuantityDecimals { get; set; } 
        /// <summary>
        /// The max amount of decimal for price
        /// </summary>
        public int? PriceDecimals { get; set; } 
    }
}
