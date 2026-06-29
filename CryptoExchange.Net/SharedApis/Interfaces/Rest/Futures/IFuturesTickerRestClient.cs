using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting ticker info for futures symbols
    /// </summary>
    public interface IFuturesTickerRestClient : ISharedClient
    {
        /// <summary>
        /// Futures get ticker request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesTickerOptions GetFuturesTickerOptions { get; }
        /// <summary>
        /// Get ticker info for a specific futures symbol, see <see cref="GetFuturesTickerOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesTicker>> GetFuturesTickerAsync(GetTickerRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures get tickers request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesTickersOptions GetFuturesTickersOptions { get; }
        /// <summary>
        /// Get ticker info for all futures symbols, see <see cref="GetFuturesTickersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesTicker[]>> GetFuturesTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }
}
