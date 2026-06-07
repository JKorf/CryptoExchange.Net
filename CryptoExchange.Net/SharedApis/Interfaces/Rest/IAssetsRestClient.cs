using CryptoExchange.Net.Objects;
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
        /// Asset request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<GetAssetRequest, IAssetsRestClient> GetAssetOptions { get; }

        /// <summary>
        /// Get info on a specific asset, see <see cref="GetAssetOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedAsset>> GetAssetAsync(GetAssetRequest request, CancellationToken ct = default);

        /// <summary>
        /// Assets request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<GetAssetsRequest, IAssetsRestClient> GetAssetsOptions { get; }

        /// <summary>
        /// Get info on all assets the exchange supports, see <see cref="GetAssetsOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedAsset[]>> GetAssetsAsync(GetAssetsRequest request, CancellationToken ct = default);
    }
}
