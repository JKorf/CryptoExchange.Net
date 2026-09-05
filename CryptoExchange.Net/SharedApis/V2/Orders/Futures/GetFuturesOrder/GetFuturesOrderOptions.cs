using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a futures order by id endpoint
    /// </summary>
    public class GetFuturesOrderOptions : CapabilityOptions<GetOrderRequest, IGetFuturesOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a futures order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOrderRequest>.Required(x => x.Symbol, "The symbol of the order to retrieve", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetOrderRequest>.Required(x => x.OrderId, "The id of the order to retrieve", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFuturesOrderRest.GetFuturesOrderAsync), _defaultParameterRules)
        {
        }
    }
}
