using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving trading history
    /// </summary>
    public interface ITradeHistoryRestClient : ISharedClient
    {
        /// <summary>
        /// Trade history request options
        /// </summary>
        GetTradeHistoryOptions GetTradeHistoryOptions { get; }

        /// <summary>
        /// Get public trade history
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<IEnumerable<SharedTrade>>> GetTradeHistoryAsync(GetTradeHistoryRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
