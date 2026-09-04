namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to cancel all currently open orders
    /// </summary>
    public record CancelAllSymbolOrdersRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol the order is on</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public CancelAllSymbolOrdersRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) 
            : base(symbol, exchangeParameters)
        {
        }
    }
}
