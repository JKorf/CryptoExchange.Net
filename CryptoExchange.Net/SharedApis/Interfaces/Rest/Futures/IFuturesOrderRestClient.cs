using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing and managing futures orders
    /// </summary>
    public interface IFuturesOrderRestClient : ISharedClient
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
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceFuturesOrderOptions PlaceFuturesOrderOptions { get; }

        /// <summary>
        /// Place a new futures order, see <see cref="PlaceFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures get order request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesOrderOptions GetFuturesOrderOptions { get; }
        /// <summary>
        /// Get info on a specific futures order, see <see cref="GetFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesOrder>> GetFuturesOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures get open orders request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetOpenFuturesOrdersOptions GetOpenFuturesOrdersOptions { get; }
        /// <summary>
        /// Get info on a open futures orders, see <see cref="GetOpenFuturesOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesOrder[]>> GetOpenFuturesOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get closed orders request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesClosedOrdersOptions GetClosedFuturesOrdersOptions { get; }
        /// <summary>
        /// Get info on closed futures orders, see <see cref="GetClosedFuturesOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesOrder[]>> GetClosedFuturesOrdersAsync(GetClosedOrdersRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Futures get order trades request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesOrderTradesOptions GetFuturesOrderTradesOptions { get; }
        /// <summary>
        /// Get trades for a specific futures order, see <see cref="GetFuturesOrderTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedUserTrade[]>> GetFuturesOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures user trades request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesUserTradesOptions GetFuturesUserTradesOptions { get; }
        /// <summary>
        /// Get futures user trade records, see <see cref="GetFuturesUserTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedUserTrade[]>> GetFuturesUserTradesAsync(GetUserTradesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Futures cancel order request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelFuturesOrderOptions CancelFuturesOrderOptions { get; }
        /// <summary>
        /// Cancel a futures order, see <see cref="CancelFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Positions request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetPositionsOptions GetPositionsOptions { get; }
        /// <summary>
        /// Get open position info, see <see cref="GetPositionsOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedPosition[]>> GetPositionsAsync(GetPositionsRequest request, CancellationToken ct = default);

        /// <summary>
        /// Close position order request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        ClosePositionOptions ClosePositionOptions { get; }
        /// <summary>
        /// Close a currently open position, see <see cref="ClosePositionOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<SharedId>> ClosePositionAsync(ClosePositionRequest request, CancellationToken ct = default);
    }
}
