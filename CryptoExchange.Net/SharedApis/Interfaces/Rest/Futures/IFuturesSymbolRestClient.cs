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
        /// Get all futures symbols for a specific base asset
        /// </summary>
        /// <param name="baseAsset">Asset, for example `ETH`</param>
        Task<ExchangeResult<SharedSymbol[]>> GetFuturesSymbolsForBaseAssetAsync(string baseAsset);

        /// <summary>
        /// Gets whether the client supports a futures symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        Task<ExchangeResult<bool>> SupportsFuturesSymbolAsync(SharedSymbol symbol);

        /// <summary>
        /// Gets whether the client supports a futures symbol
        /// </summary>
        /// <param name="symbolName">The symbol name</param>
        Task<ExchangeResult<bool>> SupportsFuturesSymbolAsync(string symbolName);

        /// <summary>
        /// Get info on all futures symbols supported on the exchange
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFuturesSymbol[]>> GetFuturesSymbolsAsync(GetSymbolsRequest request, CancellationToken ct = default);
    }
}
