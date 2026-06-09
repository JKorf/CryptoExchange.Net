namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a futures order by client order id
    /// </summary>
    public class CancelFuturesOrderByClientOrderIdOptions : EndpointOptions<CancelOrderRequest, IFuturesOrderClientIdRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesOrderClientIdRestClient.CancelFuturesOrderByClientOrderIdAsync))
        {
        }
    }
}
