using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting the index price
    /// </summary>
    public class GetAllIndexPricesOptions : CapabilityOptions<GetAllIndexPricesRequest, IGetAllIndexPricesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the index prices for all futures symbols";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetAllIndexPricesRequest>.Optional(x => x.TradingMode, "Filter the index prices by trading mode", TradingMode.PerpetualLinear),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetAllIndexPricesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetAllIndexPricesRest.GetAllIndexPricesAsync), _defaultParameterRules)
        {
        }
    }
}
