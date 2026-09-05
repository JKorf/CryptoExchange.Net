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
    public interface IPlaceFuturesOrder : ISharedApiCapability
    {
        /// <summary>
        /// How the trading fee is deducted
        /// </summary>
        SharedFeeDeductionType FuturesFeeDeductionType { get; }
        /// <summary>
        /// How the asset is determined in which the trading fee is paid
        /// </summary>
        SharedFeeAssetType FuturesFeeAssetType { get; }
        /// <summary>
        /// Supported order types for futures orders
        /// </summary>
        SharedOrderType[] FuturesSupportedOrderTypes { get; }
        /// <summary>
        /// Supported time in force types for futures orders
        /// </summary>
        SharedTimeInForce[] FuturesSupportedTimeInForce { get; }
        /// <summary>
        /// Supported quantity types for futures orders
        /// </summary>
        SharedQuantitySupport FuturesSupportedOrderQuantity { get; }

        /// <summary>
        /// Generate a new random client order id
        /// </summary>
        /// <returns></returns>
        string GenerateClientOrderId();

        /// <summary>
        /// Futures place order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceFuturesOrderOptions PlaceFuturesOrderOptions { get; }

        /// <summary>
        /// Place a new futures order, see <see cref="PlaceFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing a futures order on an exchange via the REST API.
    /// </summary>
    public interface IPlaceFuturesOrderRest : IPlaceFuturesOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing a futures order on an exchange via the WebSocket API.
    /// </summary>
    public interface IPlaceFuturesOrderSocket : IPlaceFuturesOrder, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);
    }
}
