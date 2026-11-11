using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for placing multiple futures order on an exchange in single call.
    /// </summary>
    /// <see cref="IFuturesOrderPlacementMetadata"/>
    public interface IPlaceMultipleFuturesOrdersOperation : IFuturesOrderPlacementMetadata
    {
        /// <summary>
        /// Max number of orders per request
        /// </summary>
        public int? MaxFuturesOrdersPerRequest { get; }
        /// <summary>
        /// Whether orders for different symbols are allowed in a single place order request
        /// </summary>
        public bool PlaceMultipleFuturesOrdersAllowsMultipleSymbols { get; }


        /// <summary>
        /// Futures place order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and
        /// <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the
        /// request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the
        /// request object.
        /// </summary>
        PlaceMultipleFuturesOrdersOptions PlaceMultipleFuturesOrdersOptions { get; }

        /// <summary>
        /// Place multiple new futures orders, see <see cref="PlaceMultipleFuturesOrdersOptions"/> for request options
        /// and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<IExchangeCallResult<CallResult<SharedId>[]>> PlaceMultipleFuturesOrdersAsync(
            PlaceMultipleFuturesOrdersRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing multiple futures order on an exchange in single call.
    /// </summary>
    /// <see cref="IPlaceMultipleFuturesOrdersOperation"/>
    public interface IPlaceMultipleFuturesOrders : IPlaceMultipleFuturesOrdersOperation, ISharedApiCapability
    {
    }

    /// <summary>
    /// Operation for placing multiple futures orders on an exchange via the REST API.
    /// </summary>
    public interface IPlaceMultipleFuturesOrdersRest : IPlaceMultipleFuturesOrders, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<CallResult<SharedId>[]>> PlaceMultipleFuturesOrdersAsync(
            PlaceMultipleFuturesOrdersRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing multiple futures orders on an exchange via the WebSocket API.
    /// </summary>
    public interface IPlaceMultipleFuturesOrdersSocket : IPlaceMultipleFuturesOrders, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<CallResult<SharedId>[]>> PlaceMultipleFuturesOrdersAsync(
            PlaceMultipleFuturesOrdersRequest request,
            CancellationToken ct = default);
    }
}
