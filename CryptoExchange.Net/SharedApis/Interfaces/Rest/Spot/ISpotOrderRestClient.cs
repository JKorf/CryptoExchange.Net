using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing and managing spot orders
    /// </summary>
    public interface ISpotOrderRestClient : ISharedClient
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
        PlaceSpotOrderOptions PlaceSpotOrderOptions { get; }
        /// <summary>
        /// Place a new spot order, see <see cref="PlaceSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedId>> PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get order request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotOrderOptions GetSpotOrderOptions { get; }
        /// <summary>
        /// Get info on a specific spot order, see <see cref="GetSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedSpotOrder>> GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get open orders request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetOpenSpotOrdersOptions GetOpenSpotOrdersOptions { get; }
        /// <summary>
        /// Get info on a open spot orders, see <see cref="GetOpenSpotOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedSpotOrder[]>> GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get closed orders request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotClosedOrdersOptions GetClosedSpotOrdersOptions { get; }
        /// <summary>
        /// Get info on closed spot orders, see <see cref="GetClosedSpotOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedSpotOrder[]>> GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Spot get order trades request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotOrderTradesOptions GetSpotOrderTradesOptions { get; }
        /// <summary>
        /// Get trades for a specific spot order, see <see cref="GetSpotOrderTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedUserTrade[]>> GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot user trades request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotUserTradesOptions GetSpotUserTradesOptions { get; }
        /// <summary>
        /// Get spot user trade records, see <see cref="GetSpotUserTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedUserTrade[]>> GetSpotUserTradesAsync(GetUserTradesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Spot cancel order request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelSpotOrderOptions CancelSpotOrderOptions { get; }
        /// <summary>
        /// Cancel a spot order, see <see cref="CancelSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedId>> CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }
}
