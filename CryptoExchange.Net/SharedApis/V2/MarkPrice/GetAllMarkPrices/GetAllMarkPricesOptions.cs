using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting the mark price
    /// </summary>
    public class GetAllMarkPricesOptions : CapabilityOptions<GetAllMarkPricesRequest, IGetAllMarkPricesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the mark prices for all futures symbols";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetAllMarkPricesRequest>.Optional(x => x.TradingMode, "Filter the mark prices by trading mode", TradingMode.PerpetualLinear),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetAllMarkPricesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetAllMarkPricesRest.GetAllMarkPricesAsync), _defaultParameterRules)
        {
        }
    }
}
