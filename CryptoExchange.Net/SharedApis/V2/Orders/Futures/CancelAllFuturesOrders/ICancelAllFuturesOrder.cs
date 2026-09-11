using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling all Futures orders on an exchange.
    /// </summary>
    public interface ICancelAllFuturesOrders : ISharedApiCapability
    {
        /// <summary>
        /// Futures cancel all orders request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelAllFuturesOrdersOptions CancelAllFuturesOrdersOptions { get; }
        /// <summary>
        /// Cancel all Futures orders, see <see cref="SharedApis.CancelAllFuturesOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult> CancelAllFuturesOrdersAsync(CancelAllOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling all Futures orders on an exchange via the REST API.
    /// </summary>
    public interface ICancelAllFuturesOrdersRest : ICancelAllFuturesOrders, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult> CancelAllFuturesOrdersAsync(CancelAllOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling all Futures orders on an exchange via the WebSocket API.
    /// </summary>
    public interface ICancelAllFuturesOrdersSocket : ICancelAllFuturesOrders, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult> CancelAllFuturesOrdersAsync(CancelAllOrdersRequest request, CancellationToken ct = default);

    }
}
