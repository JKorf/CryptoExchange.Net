namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve info on a specifc order
    /// </summary>
    public record GetOrderRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Id of the order
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol the order is on</param>
        /// <param name="orderId">The id of the order</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetOrderRequest(SharedSymbol symbol, string orderId, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            OrderId = orderId;
        }
    }
}
