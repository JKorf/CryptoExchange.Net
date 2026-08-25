using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Endpoint definition for retrieving an asset on an exchange.
    /// </summary>
    public interface IGetAssetEndpoint : ISharedApiEndpoint
    {
        /// <summary>
        /// Asset request options.<br />
        /// Use <see cref="EndpointOptions{TRequest, TClient}.RequiredRequestParameters"/>, <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetAssetOptions GetAssetOptions { get; }

        /// <summary>
        /// Get info on a specific asset, see <see cref="GetAssetOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedAsset>> GetAssetAsync(GetAssetRequest request, CancellationToken ct = default);
    }
}
