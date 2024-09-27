using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting user balance info
    /// </summary>
    public interface IBalanceRestClient : ISharedClient
    {
        /// <summary>
        /// Balances request options
        /// </summary>
        EndpointOptions<GetBalancesRequest> GetBalancesOptions { get; }

        /// <summary>
        /// Get balances for the user
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<IEnumerable<SharedBalance>>> GetBalancesAsync(GetBalancesRequest request, CancellationToken ct = default);
    }
}
