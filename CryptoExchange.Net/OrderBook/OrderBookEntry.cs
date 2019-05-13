namespace CryptoExchange.Net.OrderBook
{
    public class OrderBookEntry : ISymbolOrderBookEntry
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        public OrderBookEntry(decimal price, decimal quantity)
        {
            Quantity = quantity;
            Price = price;
        }
    }
}
