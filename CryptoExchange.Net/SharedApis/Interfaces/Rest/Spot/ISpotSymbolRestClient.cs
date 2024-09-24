using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting spot symbols
    /// </summary>
    public interface ISpotSymbolRestClient : ISharedClient
    {
        /// <summary>
        /// Spot symbols request options
        /// </summary>
        EndpointOptions<GetSymbolsRequest> GetSpotSymbolsOptions { get; }

        /// <summary>
        /// Get info on all available spot symbols on the exchange
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedSpotSymbol>>> GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct = default);
    }
}
