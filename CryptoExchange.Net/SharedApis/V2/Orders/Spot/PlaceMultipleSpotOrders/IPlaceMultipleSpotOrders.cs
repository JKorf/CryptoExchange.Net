using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for placing multiple spot order on an exchange in single call.
    /// </summary>
    public interface IPlaceMultipleSpotOrders : ISharedApiCapability
    {
        /// <summary>
        /// How the trading fee is deducted
        /// </summary>
        SharedFeeDeductionType SpotFeeDeductionType { get; }
        /// <summary>
        /// How the asset is determined in which the trading fee is paid
        /// </summary>
        SharedFeeAssetType SpotFeeAssetType { get; }
        /// <summary>
        /// Supported order types for spot orders
        /// </summary>
        SharedOrderType[] SpotSupportedOrderTypes { get; }
        /// <summary>
        /// Supported time in force types for placing spot orders
        /// </summary>
        SharedTimeInForce[] SpotSupportedTimeInForce { get; }
        /// <summary>
        /// Supported quantity types for placing spot orders
        /// </summary>
        SharedQuantitySupport SpotSupportedOrderQuantity { get; }
        /// <summary>
        /// Max number of orders per request
        /// </summary>
        public int? MaxSpotOrdersPerRequest { get; }
        /// <summary>
        /// Whether orders for different symbols are allowed in a single place order request
        /// </summary>
        public bool PlaceMultipleSpotOrdersAllowsMultipleSymbols { get; }

        /// <summary>
        /// Generate a new random client order id in a format that is accepted by the exchange.
        /// </summary>
        /// <returns></returns>
        string GenerateClientOrderId();

        /// <summary>
        /// Spot place order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceMultipleSpotOrdersOptions PlaceMultipleSpotOrdersOptions { get; }

        /// <summary>
        /// Place multiple new spot orders, see <see cref="PlaceMultipleSpotOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<CallResult<SharedId>[]>> PlaceMultipleSpotOrdersAsync(
            PlaceMultipleSpotOrdersRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing multiple spot orders on an exchange via the REST API.
    /// </summary>s
    public interface IPlaceMultipleSpotOrdersRest : IPlaceMultipleSpotOrders, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<CallResult<SharedId>[]>> PlaceMultipleSpotOrdersAsync(PlaceMultipleSpotOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing multiple spot orders on an exchange via the WebSocket API.
    /// </summary>
    public interface IPlaceMultipleSpotOrdersSocket : IPlaceMultipleSpotOrders, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<CallResult<SharedId>[]>> PlaceMultipleSpotOrdersAsync(PlaceMultipleSpotOrdersRequest request, CancellationToken ct = default);
    }
}
