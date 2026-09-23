namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to cancel a currently open order
    /// </summary>
    public record CancelOrderRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Id of order to cancel
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol the order is on</param>
        /// <param name="orderId">Id of the order to close</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public CancelOrderRequest(SharedSymbol symbol, string orderId, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            OrderId = orderId;
        }
    }
}
