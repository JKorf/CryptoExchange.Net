namespace CryptoExchange.Net.OrderBook
{
    /// <summary>
    /// Interface for order book entries
    /// </summary>
    public interface ISymbolOrderBookEntry
    {
        /// <summary>
        /// The quantity of the entry
        /// </summary>
        decimal Quantity { get; set; }
        /// <summary>
        /// The price of the entry
        /// </summary>
        decimal Price { get; set; }
    }
}
