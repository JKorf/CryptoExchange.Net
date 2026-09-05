using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting open positions
    /// </summary>
    public class GetPositionsOptions : CapabilityOptions<GetPositionsRequest, IGetPositionsRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open futures positions";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetPositionsRequest>.Optional(x => x.TradingMode, "Filter the result set by trading mode", TradingMode.PerpetualLinear),
            RequestParameterRule<GetPositionsRequest>.Optional(x => x.Symbol, "Filter the result set by symbol", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetPositionsRest.GetPositionsAsync), _defaultParameterRules)
        {
        }
    }
}
