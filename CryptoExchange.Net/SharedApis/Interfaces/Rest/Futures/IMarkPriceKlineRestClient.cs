using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for getting the mark price klines for a symbol
    /// </summary>
    public interface IMarkPriceKlineRestClient : ISharedClient
    {
        /// <summary>
        /// Mark price klines request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetKlinesOptions GetMarkPriceKlinesOptions { get; }
        /// <summary>
        /// Get mark price kline/candlestick data, see <see cref="GetMarkPriceKlinesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesKline[]>> GetMarkPriceKlinesAsync(GetKlinesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
