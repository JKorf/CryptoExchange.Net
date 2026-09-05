using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting trading fee info
    /// </summary>
    public class GetFeeOptions : CapabilityOptions<GetFeeRequest, IGetFeesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve trading fee information";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetFeeRequest>.Required(x => x.Symbol, "The symbol to retrieve trading fees for", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetFeeOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFeesRest.GetFeesAsync), _defaultParameterRules)
        {
        }
    }
}
