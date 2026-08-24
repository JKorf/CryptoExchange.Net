using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    public interface IGetAllAssetsRestClient : ISharedClient
    {
        /// <summary>
        /// Assets request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetAssetsOptions GetAssetsOptions { get; }

        /// <summary>
        /// Get info on all assets the exchange supports, see <see cref="GetAssetsOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedAsset[]>> GetAssetsAsync(GetAssetsRequest request, CancellationToken ct = default);
    }
}
