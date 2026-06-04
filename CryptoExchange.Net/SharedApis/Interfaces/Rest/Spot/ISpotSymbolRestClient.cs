using CryptoExchange.Net.Objects;
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
        EndpointOptions<GetSymbolsRequest, ISpotSymbolRestClient> GetSpotSymbolsOptions { get; }

        /// <summary>
        /// Get all spot symbols for a specific base asset
        /// </summary>
        /// <param name="baseAsset">Asset, for example `ETH`</param>
        Task<ExchangeCallResult<SharedSymbol[]>> GetSpotSymbolsForBaseAssetAsync(string baseAsset);

        /// <summary>
        /// Gets whether the client supports a spot symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        Task<ExchangeCallResult<bool>> SupportsSpotSymbolAsync(SharedSymbol symbol);

        /// <summary>
        /// Gets whether the client supports a spot symbol
        /// </summary>
        /// <param name="symbolName">The symbol name</param>
        Task<ExchangeCallResult<bool>> SupportsSpotSymbolAsync(string symbolName);

        /// <summary>
        /// Get info on all available spot symbols on the exchange
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedSpotSymbol[]>> GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct = default);
    }
}
