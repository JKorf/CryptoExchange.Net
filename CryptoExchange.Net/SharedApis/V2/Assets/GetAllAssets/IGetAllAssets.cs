using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving all assets supported on an exchange.
    /// </summary>
    public interface IGetAllAssets : ISharedApiCapability
    {
        /// <summary>
        /// Assets request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetAllAssetsOptions GetAllAssetsOptions { get; }

        /// <summary>
        /// Get info on all assets the exchange supports, see <see cref="GetAllAssetsOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedAsset[]>> GetAllAssetsAsync(GetAssetsRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving all assets supported on an exchange via the REST API.
    /// </summary>
    public interface IGetAllAssetsRest : IGetAllAssets, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedAsset[]>> GetAllAssetsAsync(GetAssetsRequest request, CancellationToken ct = default);
    }
}
