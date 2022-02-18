namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Order book entry
    /// </summary>
    public class OrderBookEntry
    {
        /// <summary>
        /// Quantity of the entry
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Price of the entry
        /// </summary>
        public decimal Price { get; set; }
    }
}
