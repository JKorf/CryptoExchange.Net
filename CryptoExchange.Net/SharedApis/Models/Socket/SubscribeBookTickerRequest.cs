namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to book ticker updates
    /// </summary>
    public record SubscribeBookTickerRequest : SharedSymbolRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">The symbol to subscribe to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeBookTickerRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
        }
    }
}
