namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the leverage tier information for a symbol
    /// </summary>
    public record GetLeverageTiersRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to request leverage for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetLeverageTiersRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }
    }
}
