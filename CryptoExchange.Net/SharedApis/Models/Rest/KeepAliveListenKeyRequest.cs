using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to keep-alive the update stream for the specified listen key
    /// </summary>
    public record KeepAliveListenKeyRequest : SharedRequest
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
        /// <param name="listenKey">The key to keep alive</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public KeepAliveListenKeyRequest(string listenKey, TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ListenKey = listenKey;
            TradingMode = tradingMode;
        }
    }
}
