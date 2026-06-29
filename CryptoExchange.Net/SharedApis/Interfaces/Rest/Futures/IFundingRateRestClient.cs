using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for request funding rate records
    /// </summary>
    public interface IFundingRateRestClient : ISharedClient
    {
        /// <summary>
        /// Funding rate request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFundingRateHistoryOptions GetFundingRateHistoryOptions { get; }
        /// <summary>
        /// Get funding rate records, see <see cref="GetFundingRateHistoryOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFundingRate[]>> GetFundingRateHistoryAsync(GetFundingRateHistoryRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
