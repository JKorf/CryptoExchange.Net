namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to futures order updates
    /// </summary>
    public record SubscribeFuturesOrderRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeFuturesOrderRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null)
            : base(tradingMode, exchangeParameters)
        {
        }
    }
}
