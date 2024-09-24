using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for getting the open interest for a symbol
    /// </summary>
    public interface IOpenInterestRestClient : ISharedClient
    {
        /// <summary>
        /// Open interest request options
        /// </summary>
        EndpointOptions<GetOpenInterestRequest> GetOpenInterestOptions { get; }
        /// <summary>
        /// Get the open interest for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedOpenInterest>> GetOpenInterestAsync(GetOpenInterestRequest request, CancellationToken ct = default);
    }
}
