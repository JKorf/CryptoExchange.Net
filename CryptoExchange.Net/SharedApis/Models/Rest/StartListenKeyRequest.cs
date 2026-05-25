namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to start the update stream for the current user
    /// </summary>
    public record StartListenKeyRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public StartListenKeyRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null)
            : base(tradingMode, exchangeParameters)
        {
            TradingMode = tradingMode;
        }
    }
}
