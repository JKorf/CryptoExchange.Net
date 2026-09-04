namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the current mark prices for all symbols
    /// </summary>
    public record GetMarkPricesRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetMarkPricesRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(tradingMode, exchangeParameters)
        {
        }
    }
}
