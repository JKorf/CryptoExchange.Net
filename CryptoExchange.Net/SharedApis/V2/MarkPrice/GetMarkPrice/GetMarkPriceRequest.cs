namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current mark price for a symbol
    /// </summary>
    public record GetMarkPriceRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve the mark price for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetMarkPriceRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }
    }
}
