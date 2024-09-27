using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving withdrawal records
    /// </summary>
    public interface IWithdrawalRestClient : ISharedClient
    {
        /// <summary>
        /// Withdrawal record request options
        /// </summary>
        GetWithdrawalsOptions GetWithdrawalsOptions { get; }

        /// <summary>
        /// Get withdrawal records
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<IEnumerable<SharedWithdrawal>>> GetWithdrawalsAsync(GetWithdrawalsRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
