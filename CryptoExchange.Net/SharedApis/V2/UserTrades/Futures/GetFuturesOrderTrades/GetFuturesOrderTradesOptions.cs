using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a trades for an order
    /// </summary>
    public class GetFuturesOrderTradesOptions : CapabilityOptions<GetOrderTradesRequest, IGetFuturesOrderTradesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve trades for a futures order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOrderTradesRequest>.Required(x => x.Symbol, "The symbol of the order to retrieve trades for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetOrderTradesRequest>.Required(x => x.OrderId, "The id of the order to retrieve trades for", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesOrderTradesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFuturesOrderTradesRest.GetFuturesOrderTradesAsync), _defaultParameterRules)
        {
        }
    }
}
