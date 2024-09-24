using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for getting position history
    /// </summary>
    public interface IPositionHistoryRestClient : ISharedClient
    {
        /// <summary>
        /// Position history request options
        /// </summary>
        GetPositionHistoryOptions GetPositionHistoryOptions { get; }
        /// <summary>
        /// Get position history
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedPositionHistory>>> GetPositionHistoryAsync(GetPositionHistoryRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
