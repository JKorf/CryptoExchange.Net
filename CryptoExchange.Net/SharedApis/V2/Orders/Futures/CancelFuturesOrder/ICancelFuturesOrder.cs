using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling an open futures order on an exchange.
    /// </summary>
    public interface ICancelFuturesOrder : ISharedApiCapability
    {
        /// <summary>
        /// Futures cancel order request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelFuturesOrderOptions CancelFuturesOrderOptions { get; }
        /// <summary>
        /// Cancel a futures order, see <see cref="CancelFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling an open futures order on an exchange via the REST API.
    /// </summary>
    public interface ICancelFuturesOrderRest : ICancelFuturesOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }

    /// <summary>
    /// Operation for canceling an open futures order on an exchange via the WebSocket API.
    /// </summary>
    public interface ICancelFuturesOrderSocket : ICancelFuturesOrder, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }
}
