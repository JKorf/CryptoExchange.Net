using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request definition for retrieving withdrawal history from an exchange
    /// </summary>
    public interface IGetWithdrawalHistoryEndpoint : ISharedApiEndpoint
    {
        /// <summary>
        /// Withdrawal records request options.<br />
        /// Use <see cref="EndpointOptions{TRequest, TClient}.RequiredRequestParameters"/>, <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetWithdrawalHistoryOptions GetWithdrawalHistoryOptions { get; }

        /// <summary>
        /// Get withdrawal records, see <see cref="GetWithdrawalHistoryOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the <see cref="HttpResult{T}.NextPageRequest"/> property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result <see cref="HttpResult{T}.NextPageRequest"/> property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<SharedWithdrawal[]>> GetWithdrawalHistoryAsync(GetWithdrawalsRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
