using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    public interface IPlaceSpotOrder : ISharedApiCapability
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
        /// Generate a new random client order id in a format that is accepted by the exchange.
        /// </summary>
        /// <returns></returns>
        string GenerateClientOrderId();

        /// <summary>
        /// Spot place order request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceSpotOrderOptions PlaceSpotOrderOptions { get; }

        Task<ICallResult<SharedId>> PlaceSpotOrderAsync(
            PlaceSpotOrderRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Request definition for placing a spot order on an exchange.
    /// </summary>
    public interface IPlaceSpotOrderRest : IPlaceSpotOrder, ISharedRest
    {
        /// <summary>
        /// Place a new spot order, see <see cref="PlaceSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<HttpResult<SharedId>> PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Client for placing and managing spot orders
    /// </summary>
    public interface IPlaceSpotOrderSocket : IPlaceSpotOrder, ISharedSocket
    {
        /// <summary>
        /// Place a new spot order, see <see cref="PlaceSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<QueryResult<SharedId>> PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct = default);
    }
}
