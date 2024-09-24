namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current order book
    /// </summary>
    public record GetOrderBookRequest: SharedSymbolRequest
    {
        /// <summary>
        /// Depth of the order book
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol the order is on</param>
        /// <param name="limit">Depth of the order book</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetOrderBookRequest(SharedSymbol symbol, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Limit = limit;
        }
    }
}
