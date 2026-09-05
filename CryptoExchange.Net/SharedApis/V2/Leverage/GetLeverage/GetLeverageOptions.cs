using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting asset info
    /// </summary>
    public class GetLeverageOptions : CapabilityOptions<GetLeverageRequest, IGetLeverageRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the current leverage for a futures symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetLeverageRequest>.Required(x => x.Symbol, "The symbol to retrieve leverage for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetLeverageRequest>.Optional(x => x.PositionSide, "The position side to retrieve leverage for", SharedPositionSide.Long),
            RequestParameterRule<GetLeverageRequest>.Optional(x => x.MarginMode, "The margin mode to retrieve leverage for", SharedMarginMode.Cross),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetLeverageOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetLeverageRest.GetLeverageAsync), _defaultParameterRules)
        {
        }
    }
}
