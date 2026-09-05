using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for editing an open Futures order on an exchange.
    /// </summary>
    public interface IEditFuturesOrder : ISharedApiCapability
    {
        /// <summary>
        /// Futures edit order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EditFuturesOrderOptions EditFuturesOrderOptions { get; }
        
        /// <summary>
        /// Edit an existing Futures order, see <see cref="EditFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> EditFuturesOrderAsync(
            EditFuturesOrderRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing a Futures order on an exchange via the REST API.
    /// </summary>
    public interface IEditFuturesOrderRest : IEditFuturesOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> EditFuturesOrderAsync(EditFuturesOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing a Futures order on an exchange via the WebSocket API.
    /// </summary>
    public interface IEditFuturesOrderSocket : IEditFuturesOrder, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> EditFuturesOrderAsync(EditFuturesOrderRequest request, CancellationToken ct = default);
    }
}
