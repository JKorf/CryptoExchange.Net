using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing and managing spot orders
    /// </summary>
    public interface ISpotOrderManagementSocketClient : ISharedClient
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
        /// Generate a new random client order id
        /// </summary>
        /// <returns></returns>
        string GenerateClientOrderId();

        /// <summary>
        /// Spot place order request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceSpotOrderSocketOptions PlaceSpotOrderOptions { get; }
        /// <summary>
        /// Place a new spot order, see <see cref="PlaceSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<QueryResult<SharedId>> PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot cancel order request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelSpotOrderSocketOptions CancelSpotOrderOptions { get; }
        /// <summary>
        /// Cancel a spot order, see <see cref="CancelSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<QueryResult<SharedId>> CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }
}
