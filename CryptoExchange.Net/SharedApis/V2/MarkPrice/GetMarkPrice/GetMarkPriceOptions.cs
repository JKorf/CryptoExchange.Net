using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting the mark price
    /// </summary>
    public class GetMarkPriceOptions : CapabilityOptions<GetMarkPriceRequest, IGetMarkPriceRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the mark price for a futures symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetMarkPriceRequest>.Required(x => x.Symbol, "The symbol to retrieve the mark price for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetMarkPriceOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetMarkPriceRest.GetMarkPriceAsync), _defaultParameterRules)
        {
        }
    }
}
