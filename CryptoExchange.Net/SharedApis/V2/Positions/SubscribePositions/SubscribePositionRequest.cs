namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to position updates
    /// </summary>
    public record SubscribePositionRequest: SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribePositionRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null)
            : base(tradingMode, exchangeParameters)
        {
        }
    }
}
