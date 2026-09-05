using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by client order id
    /// </summary>
    public class GetFuturesOrderByClientOrderIdOptions : CapabilityOptions<GetOrderRequest, IGetFuturesOrderByClientOrderIdRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a futures order by its client order id";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOrderRequest>.Required(x => x.Symbol, "The symbol of the order to retrieve", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetOrderRequest>.Required(x => x.OrderId, "The id of the order to retrieve", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFuturesOrderByClientOrderIdRest.GetFuturesOrderByClientOrderIdAsync), _defaultParameterRules)
        {
        }
    }
}
