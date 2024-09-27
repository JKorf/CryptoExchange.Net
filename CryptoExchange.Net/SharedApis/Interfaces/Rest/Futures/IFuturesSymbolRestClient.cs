using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for request futures symbol info
    /// </summary>
    public interface IFuturesSymbolRestClient : ISharedClient
    {
        /// <summary>
        /// Futures symbol request options
        /// </summary>
        EndpointOptions<GetSymbolsRequest> GetFuturesSymbolsOptions { get; }
        /// <summary>
        /// Get info on all futures symbols supported on the exchagne
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedFuturesSymbol>>> GetFuturesSymbolsAsync(GetSymbolsRequest request, CancellationToken ct = default);
    }
}
