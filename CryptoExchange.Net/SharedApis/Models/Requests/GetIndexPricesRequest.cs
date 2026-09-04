namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current index prices for all symbols
    /// </summary>
    public record GetIndexPricesRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetIndexPricesRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(tradingMode, exchangeParameters)
        {
        }
    }
}
