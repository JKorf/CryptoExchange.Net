using System.Collections.Generic;
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
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedFundingRate>>> GetFundingRateHistoryAsync(GetFundingRateHistoryRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
