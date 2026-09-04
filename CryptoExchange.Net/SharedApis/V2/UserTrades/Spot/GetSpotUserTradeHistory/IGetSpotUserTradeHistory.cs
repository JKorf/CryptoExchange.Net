using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving spot user trade history from an exchange.
    /// </summary>
    public interface IGetSpotUserTradeHistory : ISharedApiCapability
    {
        /// <summary>
        /// Spot user trades request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotUserTradeHistoryOptions GetSpotUserTradeHistoryOptions { get; }
        /// <summary>
        /// Get spot user trade records, see <see cref="SharedApis.GetSpotUserTradeHistoryOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the <see cref="HttpResult{T}.NextPageRequest"/> property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result <see cref="HttpResult{T}.NextPageRequest"/> property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedUserTrade[]>> GetSpotUserTradeHistoryAsync(GetUserTradesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving spot user trade history from an exchange via the REST API.
    /// </summary>
    public interface IGetSpotUserTradeHistoryRest : IGetSpotUserTradeHistory, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedUserTrade[]>> GetSpotUserTradeHistoryAsync(GetUserTradesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
