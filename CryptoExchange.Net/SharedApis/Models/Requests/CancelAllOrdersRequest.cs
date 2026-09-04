namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to cancel all currently open orders
    /// </summary>
    public record CancelAllOrdersRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public CancelAllOrdersRequest(ExchangeParameters? exchangeParameters = null) : base(null, exchangeParameters)
        {
        }
    }
}
