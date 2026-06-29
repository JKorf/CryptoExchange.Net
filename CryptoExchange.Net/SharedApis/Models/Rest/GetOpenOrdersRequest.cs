namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current open orders
    /// </summary>
    public record GetOpenOrdersRequest : SharedRequest
    {
        /// <summary>
        /// Symbol filter
        /// </summary>
        public SharedSymbol? Symbol { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetOpenOrdersRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(tradingMode, exchangeParameters)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve open orders for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetOpenOrdersRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol.TradingMode, exchangeParameters)
        {
            Symbol = symbol;
        }
    }
}
