namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to user trade updates
    /// </summary>
    public record SubscribeUserTradeRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeUserTradeRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null)
            : base(tradingMode, exchangeParameters)
        {
        }
    }
}
