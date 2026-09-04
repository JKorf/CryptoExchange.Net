using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling an open futures order by its client order id on an exchange.
    /// </summary>
    public interface ICancelFuturesOrderByClientOrderId : ISharedApiCapability
    {
        /// <summary>
        /// Futures cancel order by client order id request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelFuturesOrderByClientOrderIdOptions CancelFuturesOrderByClientOrderIdOptions { get; }
        /// <summary>
        /// Cancel a futures order using client order id, see <see cref="CancelFuturesOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling an open futures order by its client order id on an exchange via the REST API.
    /// </summary>
    public interface ICancelFuturesOrderByClientOrderIdRest : ICancelFuturesOrderByClientOrderId, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling an open futures order by its client order id on an exchange via the WebSocket API.
    /// </summary>
    public interface ICancelFuturesOrderByClientOrderIdSocket : ICancelFuturesOrderByClientOrderId, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
