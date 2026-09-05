using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by id endpoint
    /// </summary>
    public class GetOpenSpotOrdersOptions : CapabilityOptions<GetOpenOrdersRequest, IGetOpenSpotOrdersRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open spot orders";
        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOpenOrdersRequest>.Optional(x => x.TradingMode, "Filter the open orders by trading mode", TradingMode.Spot),
            RequestParameterRule<GetOpenOrdersRequest>.Optional(x => x.Symbol, "The symbol to retrieve open spot orders for", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetOpenSpotOrdersOptions(string exchange, bool authenticated)
            : base(exchange, authenticated, nameof(IGetOpenSpotOrdersRest.GetOpenSpotOrdersAsync), _defaultParameterRules)
        {
        }
    }
}
