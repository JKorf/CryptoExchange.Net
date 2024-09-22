using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Options.Endpoints
{
    /// <summary>
    /// Options for requesting position history
    /// </summary>
    public class GetPositionHistoryOptions : PaginatedEndpointOptions<GetPositionHistoryRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionHistoryOptions(SharedPaginationSupport paginationType) : base(paginationType, true)
        {
        }
    }
}
