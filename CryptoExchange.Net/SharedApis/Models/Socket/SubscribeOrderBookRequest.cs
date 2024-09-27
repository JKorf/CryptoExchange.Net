namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to order book snapshot updates
    /// </summary>
    public record SubscribeOrderBookRequest : SharedSymbolRequest
    {
        /// <summary>
        /// The order book depth
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="limit">Order book depth</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeOrderBookRequest(SharedSymbol symbol, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Limit = limit;
        }
    }
}
