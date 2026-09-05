using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting open interest
    /// </summary>
    public class GetOpenInterestOptions : CapabilityOptions<GetOpenInterestRequest, IGetOpenInterestRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open interest for a futures symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOpenInterestRequest>.Required(x => x.Symbol, "The symbol to retrieve open interest for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetOpenInterestOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetOpenInterestRest.GetOpenInterestAsync), _defaultParameterRules)
        {
        }
    }
}
