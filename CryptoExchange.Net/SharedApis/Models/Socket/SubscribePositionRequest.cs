using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to position updates
    /// </summary>
    public record SubscribePositionRequest: SharedRequest
    {
        /// <summary>
        /// The listen key, needed for some exchanges. Can be obtained by the StartListenKeyAsync on the shared rest client
        /// </summary>
        public string? ListenKey { get; set; }
        /// <summary>
        /// Trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="listenKey">Listen key, needed for some exchanges. Can be obtained by the StartListenKeyAsync on the shared rest client</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribePositionRequest(string? listenKey = null, TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            TradingMode = tradingMode;
            ListenKey = listenKey;
        }
    }
}
