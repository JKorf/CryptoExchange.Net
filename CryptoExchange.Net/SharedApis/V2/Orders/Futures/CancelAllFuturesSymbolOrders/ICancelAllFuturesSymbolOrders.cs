using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling all open futures orders for a specific symbol on an exchange.
    /// </summary>
    public interface ICancelAllFuturesSymbolOrders : ISharedApiCapability
    {
        /// <summary>
        /// Futures cancel order request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelAllFuturesSymbolOrdersOptions CancelAllFuturesSymbolOrdersOptions { get; }
        /// <summary>
        /// Cancel all open futures orders for a specific symbol, see <see cref="CancelAllFuturesSymbolOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult> CancelAllFuturesSymbolOrdersAsync(CancelAllSymbolOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling all open futures orders for a specific symbol on an exchange via the REST API.
    /// </summary>
    public interface ICancelAllFuturesSymbolOrdersRest : ICancelAllFuturesSymbolOrders, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult> CancelAllFuturesSymbolOrdersAsync(CancelAllSymbolOrdersRequest request, CancellationToken ct = default);

    }

    /// <summary>
    /// Operation for canceling all open futures orders for a specific symbol on an exchange via the WebSocket API.
    /// </summary>
    public interface ICancelAllFuturesSymbolOrdersSocket : ICancelAllFuturesSymbolOrders, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> CancelAllFuturesSymbolOrdersAsync(CancelAllSymbolOrdersRequest request, CancellationToken ct = default);

    }
}
