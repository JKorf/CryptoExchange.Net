using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a futures order by id endpoint
    /// </summary>
    public class GetOpenFuturesOrdersOptions : CapabilityOptions<GetOpenOrdersRequest, IGetOpenFuturesOrdersRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open futures orders";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOpenOrdersRequest>.Optional(x => x.TradingMode, "Filter the open orders by trading mode", TradingMode.PerpetualLinear),
            RequestParameterRule<GetOpenOrdersRequest>.Optional(x => x.Symbol, "The symbol to retrieve open orders for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetOpenFuturesOrdersOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetOpenFuturesOrdersRest.GetOpenFuturesOrdersAsync), _defaultParameterRules)
        {
        }
    }
}
