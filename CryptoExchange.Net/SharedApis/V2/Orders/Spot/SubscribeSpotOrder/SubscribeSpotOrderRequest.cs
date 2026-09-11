namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to spot order updates
    /// </summary>
    public record SubscribeSpotOrderRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeSpotOrderRequest(ExchangeParameters? exchangeParameters = null) 
            : base(SharedApis.TradingMode.Spot, exchangeParameters)
        {
        }
    }
}
