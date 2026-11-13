using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for placing a futures order on an exchange.
    /// </summary>
    /// <see cref="IFuturesOrderPlacementMetadata"/>
    public interface IPlaceFuturesOrderOperation : IFuturesOrderPlacementMetadata
    {
        /// <summary>
        /// Futures place order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and
        /// <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the
        /// request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the
        /// request object.
        /// </summary>
        PlaceFuturesOrderOptions PlaceFuturesOrderOptions { get; }

        /// <summary>
        /// Place a new futures order, see <see cref="PlaceFuturesOrderOptions"/> for request options and exchange
        /// specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<IExchangeCallResult<SharedId>> PlaceFuturesOrderAsync(
            PlaceFuturesOrderRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing a futures order on an exchange.
    /// </summary>
    /// <see cref="IPlaceFuturesOrderOperation"/>
    public interface IPlaceFuturesOrder : IPlaceFuturesOrderOperation, ISharedApiCapability
    {
    }

    /// <summary>
    /// Operation for placing a futures order on an exchange via the REST API.
    /// </summary>
    public interface IPlaceFuturesOrderRest : IPlaceFuturesOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> PlaceFuturesOrderAsync(
            PlaceFuturesOrderRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing a futures order on an exchange via the WebSocket API.
    /// </summary>
    public interface IPlaceFuturesOrderSocket : IPlaceFuturesOrder, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> PlaceFuturesOrderAsync(
            PlaceFuturesOrderRequest request,
            CancellationToken ct = default);
    }
}
