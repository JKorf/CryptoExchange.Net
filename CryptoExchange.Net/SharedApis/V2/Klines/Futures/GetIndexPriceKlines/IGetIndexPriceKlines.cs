using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving index price kline/candlestick data from an exchange
    /// </summary>
    public interface IGetIndexPriceKlines : ISharedApiCapability
    {
        /// <summary>
        /// Index price klines request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetIndexPriceKlinesOptions GetIndexPriceKlinesOptions { get; }
        /// <summary>
        /// Get index price kline/candlestick data, see <see cref="GetIndexPriceKlinesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the <see cref="HttpResult{T}.NextPageRequest"/> property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result <see cref="HttpResult{T}.NextPageRequest"/> property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesKline[]>> GetIndexPriceKlinesAsync(GetKlinesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving index price kline/candlestick data from an exchange via the REST API.
    /// </summary>
    public interface IGetIndexPriceKlinesRest : IGetIndexPriceKlines, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFuturesKline[]>> GetIndexPriceKlinesAsync(GetKlinesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
