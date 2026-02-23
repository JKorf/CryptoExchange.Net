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
        /// Funding rate request options
        /// </summary>
        GetFundingRateHistoryOptions GetFundingRateHistoryOptions { get; }
        /// <summary>
        /// Get funding rate records
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFundingRate[]>> GetFundingRateHistoryAsync(GetFundingRateHistoryRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
