namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to futures order updates
    /// </summary>
    public record SubscribeFuturesOrderRequest : SharedRequest
    {
        /// <summary>
        /// The listen key, needed for some exchanges. Can be obtained by the StartListenKeyAsync on the shared rest client
        /// </summary>
        public string? ListenKey { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="listenKey">Listen key, needed for some exchanges. Can be obtained by the StartListenKeyAsync on the shared rest client</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeFuturesOrderRequest(string? listenKey = null, TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null)
            : base(tradingMode, exchangeParameters)
        {
            ListenKey = listenKey;
        }
    }
}
