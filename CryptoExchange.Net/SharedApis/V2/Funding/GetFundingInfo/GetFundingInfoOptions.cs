using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting funding info
    /// </summary>
    public class GetFundingInfoOptions : PaginatedCapabilityOptions<GetFundingInfoRequest, IGetFundingInfoRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the current funding info for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetFundingInfoRequest>.Required(x => x.Symbol, "The symbol", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetFundingInfoOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit, bool needsAuthentication) 
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, needsAuthentication, nameof(IGetFundingInfoRest.GetFundingInfoAsync), _defaultParameterRules)
        {
        }
    }
}
