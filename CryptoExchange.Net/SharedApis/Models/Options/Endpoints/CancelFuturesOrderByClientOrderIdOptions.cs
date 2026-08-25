namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a futures order by client order id
    /// </summary>
    public class CancelFuturesOrderByClientOrderIdOptions : EndpointOptions<CancelOrderRequest, ICancelFuturesOrderByClientOrderIdEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a futures order by its client order id";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelFuturesOrderByClientOrderIdEndpoint.CancelFuturesOrderByClientOrderIdAsync))
        {
        }
    }
}
