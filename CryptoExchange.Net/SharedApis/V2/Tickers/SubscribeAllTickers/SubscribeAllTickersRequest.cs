namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to ticker updates for all symbols
    /// </summary>
    public record SubscribeAllTickersRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeAllTickersRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(tradingMode, exchangeParameters)
        {
        }
    }
}
