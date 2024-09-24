namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to kline/candlestick updates
    /// </summary>
    public record SubscribeKlineRequest : SharedSymbolRequest
    {
        /// <summary>
        /// The kline interval
        /// </summary>
        public SharedKlineInterval Interval { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="interval">Kline interval</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeKlineRequest(SharedSymbol symbol, SharedKlineInterval interval, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Interval = interval;
        }
    }
}
