namespace CryptoExchange.Net.OrderBook
{
    /// <summary>
    /// Order book entry
    /// </summary>
    public class OrderBookEntry : ISymbolOrderBookEntry
    {
        /// <summary>
        /// Quantity of the entry
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Price of the entry
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        public OrderBookEntry(decimal price, decimal quantity)
        {
            Quantity = quantity;
            Price = price;
        }
    }
}
