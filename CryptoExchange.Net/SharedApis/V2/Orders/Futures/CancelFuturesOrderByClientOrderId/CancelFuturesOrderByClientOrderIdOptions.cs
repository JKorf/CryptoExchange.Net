namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a futures order by client order id
    /// </summary>
    public class CancelFuturesOrderByClientOrderIdOptions : CapabilityOptions<CancelOrderRequest, ICancelFuturesOrderByClientOrderIdRest>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a futures order by its client order id";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CancelOrderRequest>.Required(x => x.Symbol, "The symbol of the order to cancel", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<CancelOrderRequest>.Required(x => x.OrderId, "The id of the order to cancel", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelFuturesOrderByClientOrderIdRest.CancelFuturesOrderByClientOrderIdAsync), _defaultParameterRules)
        {
        }
    }
}
