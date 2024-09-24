namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to subscribe to spot order updates
    /// </summary>
    public record SubscribeSpotOrderRequest : SharedRequest
    {
        /// <summary>
        /// The listen key, needed for some exchanges. Can be obtained by the StartListenKeyAsync on the shared rest client
        /// </summary>
        public string? ListenKey { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="listenKey">Listen key, needed for some exchanges. Can be obtained by the StartListenKeyAsync on the shared rest client</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SubscribeSpotOrderRequest(string? listenKey = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ListenKey = listenKey;
        }
    }
}
