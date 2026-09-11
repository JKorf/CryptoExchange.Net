using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for editing an open futures order by its client order id on an exchange.
    /// </summary>
    public interface IEditFuturesOrderByClientOrderId : ISharedApiCapability
    {
        /// <summary>
        /// Futures edit order by client order id request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EditFuturesOrderByClientOrderIdOptions EditFuturesOrderByClientOrderIdOptions { get; }
        /// <summary>
        /// Edit a futures order using client order id, see <see cref="EditFuturesOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> EditFuturesOrderByClientOrderIdAsync(EditOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing an open futures order by its client order id on an exchange via the REST API.
    /// </summary>
    public interface IEditFuturesOrderByClientOrderIdRest : IEditFuturesOrderByClientOrderId, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> EditFuturesOrderByClientOrderIdAsync(EditOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing an open futures order by its client order id on an exchange via the WebSocket API.
    /// </summary>
    public interface IEditFuturesOrderByClientOrderIdSocket : IEditFuturesOrderByClientOrderId, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> EditFuturesOrderByClientOrderIdAsync(EditOrderRequest request, CancellationToken ct = default);
    }
}
