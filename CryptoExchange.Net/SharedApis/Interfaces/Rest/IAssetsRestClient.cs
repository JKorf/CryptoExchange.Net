using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting asset info
    /// </summary>
    public interface IAssetsRestClient : ISharedClient
    {
        /// <summary>
        /// Asset request options
        /// </summary>
        EndpointOptions<GetAssetRequest> GetAssetOptions { get; }

        /// <summary>
        /// Get info on a specific asset
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedAsset>> GetAssetAsync(GetAssetRequest request, CancellationToken ct = default);

        /// <summary>
        /// Assets request options
        /// </summary>
        EndpointOptions<GetAssetsRequest> GetAssetsOptions { get; }

        /// <summary>
        /// Get info on all assets the exchange supports
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedAsset>>> GetAssetsAsync(GetAssetsRequest request, CancellationToken ct = default);
    }
}
