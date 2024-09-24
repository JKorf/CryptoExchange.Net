using System.Collections.Generic;
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
        /// Futures get ticker request options
        /// </summary>
        EndpointOptions<GetTickerRequest> GetFuturesTickerOptions { get; }
        /// <summary>
        /// Get ticker info for a specific futures symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFuturesTicker>> GetFuturesTickerAsync(GetTickerRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures get tickers request options
        /// </summary>
        EndpointOptions<GetTickersRequest> GetFuturesTickersOptions { get; }
        /// <summary>
        /// Get ticker info for aall futures symbols
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedFuturesTicker>>> GetFuturesTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }
}
