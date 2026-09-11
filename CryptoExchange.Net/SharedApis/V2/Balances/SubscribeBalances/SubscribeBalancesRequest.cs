namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to balance updates
    /// </summary>
    public record SubscribeBalancesRequest: SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeBalancesRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) 
            : base(tradingMode, exchangeParameters)
        {
        }
    }
}
