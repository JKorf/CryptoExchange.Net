namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve ticker info for a symbol
    /// </summary>
    public record GetTickerRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve ticker for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetTickerRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }
    }
}
