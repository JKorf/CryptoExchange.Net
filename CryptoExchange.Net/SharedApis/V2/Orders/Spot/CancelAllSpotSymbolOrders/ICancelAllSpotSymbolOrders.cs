using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling all open Spot orders for a specific symbol on an exchange.
    /// </summary>
    public interface ICancelAllSpotSymbolOrders : ISharedApiCapability
    {
        /// <summary>
        /// Spot cancel order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelAllSpotSymbolOrdersOptions CancelAllSpotSymbolOrdersOptions { get; }
        /// <summary>
        /// Cancel all open Spot orders for a specific symbol, see <see cref="CancelAllSpotSymbolOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult> CancelAllSpotSymbolOrdersAsync(CancelAllSymbolOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling all open Spot orders for a specific symbol on an exchange via the REST API.
    /// </summary>
    public interface ICancelAllSpotSymbolOrdersRest : ICancelAllSpotSymbolOrders, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult> CancelAllSpotSymbolOrdersAsync(CancelAllSymbolOrdersRequest request, CancellationToken ct = default);

    }

    /// <summary>
    /// Operation for canceling all open Spot orders for a specific symbol on an exchange via the WebSocket API.
    /// </summary>
    public interface ICancelAllSpotSymbolOrdersSocket : ICancelAllSpotSymbolOrders, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> CancelAllSpotSymbolOrdersAsync(CancelAllSymbolOrdersRequest request, CancellationToken ct = default);

    }
}
