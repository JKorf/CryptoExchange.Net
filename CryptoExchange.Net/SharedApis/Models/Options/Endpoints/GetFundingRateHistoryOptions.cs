using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;

namespace CryptoExchange.Net.SharedApis.Models.Options.Endpoints
{
    /// <summary>
    /// Options for requesting funding rate history
    /// </summary>
    public class GetFundingRateHistoryOptions : PaginatedEndpointOptions<GetFundingRateHistoryRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetFundingRateHistoryOptions(SharedPaginationSupport paginationType, bool needsAuthentication) : base(paginationType, needsAuthentication)
        {
        }
    }
}
