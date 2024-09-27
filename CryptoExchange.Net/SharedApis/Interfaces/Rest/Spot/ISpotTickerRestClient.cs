using System.Collections.Generic;
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
        /// Spot ticker request options
        /// </summary>
        EndpointOptions<GetTickerRequest> GetSpotTickerOptions { get; }
        /// <summary>
        /// Get ticker for a specific spot symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedSpotTicker>> GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct = default);
        /// <summary>
        /// Spot tickers request options
        /// </summary>
        EndpointOptions<GetTickersRequest> GetSpotTickersOptions { get; }
        /// <summary>
        /// Get tickers for all spot symbols
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedSpotTicker>>> GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }
}
