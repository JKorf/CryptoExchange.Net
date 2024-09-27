using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting deposit addresses and deposit records
    /// </summary>
    public interface IDepositRestClient : ISharedClient
    {
        /// <summary>
        /// Deposit addresses request options
        /// </summary>
        EndpointOptions<GetDepositAddressesRequest> GetDepositAddressesOptions { get; }

        /// <summary>
        /// Get deposit addresses for an asset
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<IEnumerable<SharedDepositAddress>>> GetDepositAddressesAsync(GetDepositAddressesRequest request, CancellationToken ct = default);

        /// <summary>
        /// Deposits request options
        /// </summary>
        GetDepositsOptions GetDepositsOptions { get; }

        /// <summary>
        /// Get deposit records
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<IEnumerable<SharedDeposit>>> GetDepositsAsync(GetDepositsRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
