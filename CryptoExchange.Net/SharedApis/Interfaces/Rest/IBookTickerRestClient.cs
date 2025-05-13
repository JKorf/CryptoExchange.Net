using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving the current best bid/ask price
    /// </summary>
    public interface IBookTickerRestClient : ISharedClient
    {
        /// <summary>
        /// Book ticker request options
        /// </summary>
        EndpointOptions<GetBookTickerRequest> GetBookTickerOptions { get; }

        /// <summary>
        /// Get the best ask/bid info for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<SharedBookTicker>> GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct = default);
    }
}
