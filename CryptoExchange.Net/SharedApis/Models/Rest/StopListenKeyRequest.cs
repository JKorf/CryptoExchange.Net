using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to stop the update stream for the specific listen key
    /// </summary>
    public record StopListenKeyRequest : SharedRequest
    {
        /// <summary>
        /// The key to stop updates for
        /// </summary>
        public string ListenKey { get; set; }
        /// <summary>
        /// Trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="listenKey">The key to stop updates for</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public StopListenKeyRequest(string listenKey, TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ListenKey = listenKey;
            TradingMode = tradingMode;
        }
    }
}
