using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling a spot order on an exchange.
    /// </summary>
    public interface ICancelSpotOrder : ISharedApiCapability
    {
        /// <summary>
        /// Spot cancel order request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelSpotOrderOptions CancelSpotOrderOptions { get; }
        /// <summary>
        /// Cancel a spot order, see <see cref="CancelSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling a spot order on an exchange via the REST API.
    /// </summary>
    public interface ICancelSpotOrderRest : ICancelSpotOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling a spot order on an exchange via the WebSocket API.
    /// </summary>
    public interface ICancelSpotOrderSocket : ICancelSpotOrder, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }
}
