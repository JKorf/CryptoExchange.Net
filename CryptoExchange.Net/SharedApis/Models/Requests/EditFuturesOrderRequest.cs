namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to place a new Futures order
    /// </summary>
    public record EditFuturesOrderRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Order id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// Quantity of the order
        /// </summary>
        public SharedQuantity? Quantity { get; set; }
        /// <summary>
        /// Price of the order
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to place the order on</param>
        /// <param name="orderId">The id of the order to edit</param>
        /// <param name="quantity">New quantity of the order</param>
        /// <param name="price">New price of the order</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public EditFuturesOrderRequest(
            SharedSymbol symbol,
            string orderId,
            SharedQuantity? quantity = null,
            decimal? price = null,
            ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            OrderId = orderId;
            Quantity = quantity;
            Price = price;
        }
    }
}
