namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve all symbol tickers
    /// </summary>
    public record GetTickersRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetTickersRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(tradingMode, exchangeParameters)
        {
        }
    }
}
