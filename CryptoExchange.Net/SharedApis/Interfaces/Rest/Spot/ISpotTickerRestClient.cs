using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting spot tickers
    /// </summary>
    public interface ISpotTickerRestClient : ISharedClient
    {
        /// <summary>
        /// Spot ticker request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotTickerOptions GetSpotTickerOptions { get; }
        /// <summary>
        /// Get ticker for a specific spot symbol, see <see cref="GetSpotTickerOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedSpotTicker>> GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct = default);
        /// <summary>
        /// Spot tickers request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotTickersOptions GetSpotTickersOptions { get; }
        /// <summary>
        /// Get tickers for all spot symbols, see <see cref="GetSpotTickersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedSpotTicker[]>> GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }
}
