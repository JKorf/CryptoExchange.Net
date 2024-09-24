namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current open interest for a symbol
    /// </summary>
    public record GetOpenInterestRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve open orders for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetOpenInterestRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }
    }
}
