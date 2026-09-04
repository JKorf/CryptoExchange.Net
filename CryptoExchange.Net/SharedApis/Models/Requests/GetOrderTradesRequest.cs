namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the trades for a specific order
    /// </summary>
    public record GetOrderTradesRequest : SharedSymbolRequest
    {
        /// <summary>
        /// The id of the order to retrieve trades for
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol the order is on</param>
        /// <param name="orderId">The id of the order</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetOrderTradesRequest(SharedSymbol symbol, string orderId, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            OrderId = orderId;
        }
    }
}
