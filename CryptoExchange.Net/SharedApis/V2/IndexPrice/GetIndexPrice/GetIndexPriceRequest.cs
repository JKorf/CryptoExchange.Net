namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current index price for a symbol
    /// </summary>
    public record GetIndexPriceRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve the index price for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetIndexPriceRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }
    }
}
