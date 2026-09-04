using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving a spot order by client order id from an exchange.
    /// </summary>
    public interface IGetSpotOrderByClientOrderId : ISharedApiCapability
    {
        /// <summary>
        /// Spot get order by client order id request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotOrderByClientOrderIdOptions GetSpotOrderByClientOrderIdOptions { get; }

        /// <summary>
        /// Get info on a specific spot order using a client order id, see <see cref="GetSpotOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedSpotOrder>> GetSpotOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct = default);

    }

    /// <summary>
    /// Operation for retrieving a spot order by client order id from an exchange via the REST API.
    /// </summary>
    public interface IGetSpotOrderByClientOrderIdRest : IGetSpotOrderByClientOrderId, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedSpotOrder>> GetSpotOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct = default);

    }
}
