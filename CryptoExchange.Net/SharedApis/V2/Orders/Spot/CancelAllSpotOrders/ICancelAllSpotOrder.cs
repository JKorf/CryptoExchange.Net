using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling all spot orders on an exchange.
    /// </summary>
    public interface ICancelAllSpotOrders : ISharedApiCapability
    {
        /// <summary>
        /// Spot cancel all orders request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelAllSpotOrdersOptions CancelAllSpotOrdersOptions { get; }
        /// <summary>
        /// Cancel all spot orders, see <see cref="SharedApis.CancelAllSpotOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult> CancelAllSpotOrdersAsync(CancelAllOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling all spot orders on an exchange via the REST API.
    /// </summary>
    public interface ICancelAllSpotOrdersRest : ICancelAllSpotOrders, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult> CancelAllSpotOrdersAsync(CancelAllOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling all spot orders on an exchange via the WebSocket API.
    /// </summary>
    public interface ICancelAllSpotOrdersSocket : ICancelAllSpotOrders, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult> CancelAllSpotOrdersAsync(CancelAllOrdersRequest request, CancellationToken ct = default);

    }
}
