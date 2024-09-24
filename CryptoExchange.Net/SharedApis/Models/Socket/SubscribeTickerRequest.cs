namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to ticker updates
    /// </summary>
    public record SubscribeTickerRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeTickerRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }
    }
}
