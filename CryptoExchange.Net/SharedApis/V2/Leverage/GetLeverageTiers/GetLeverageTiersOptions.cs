using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting leverage tier info
    /// </summary>
    public class GetLeverageTiersOptions : CapabilityOptions<GetLeverageTiersRequest, IGetLeverageTiers>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the current leverage tier information for a futures symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetLeverageTiersRequest>.Required(x => x.Symbol, "The symbol to retrieve leverage tiers for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetLeverageTiersOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetLeverageTiersRest.GetLeverageTiersAsync), _defaultParameterRules, SharedTradingModeSets.Futures)
        {
        }
    }
}
