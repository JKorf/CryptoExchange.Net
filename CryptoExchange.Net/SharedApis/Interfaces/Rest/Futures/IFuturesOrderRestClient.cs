using System.Collections.Generic;
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
        /// Supported order types
        /// </summary>
        IEnumerable<SharedOrderType> FuturesSupportedOrderTypes { get; }
        /// <summary>
        /// Supported time in force
        /// </summary>
        IEnumerable<SharedTimeInForce> FuturesSupportedTimeInForce { get; }
        /// <summary>
        /// Quantity types support
        /// </summary>
        SharedQuantitySupport FuturesSupportedOrderQuantity { get; }

        /// <summary>
        /// Futures place order request options
        /// </summary>
        PlaceFuturesOrderOptions PlaceFuturesOrderOptions { get; }
        /// <summary>
        /// Place a new futures order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures get order request options
        /// </summary>
        EndpointOptions<GetOrderRequest> GetFuturesOrderOptions { get; }
        /// <summary>
        /// Get info on a specific futures order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFuturesOrder>> GetFuturesOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures get open orders request options
        /// </summary>
        EndpointOptions<GetOpenOrdersRequest> GetOpenFuturesOrdersOptions { get; }
        /// <summary>
        /// Get info on a open futures orders
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedFuturesOrder>>> GetOpenFuturesOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get closed orders request options
        /// </summary>
        PaginatedEndpointOptions<GetClosedOrdersRequest> GetClosedFuturesOrdersOptions { get; }
        /// <summary>
        /// Get info on closed futures orders
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedFuturesOrder>>> GetClosedFuturesOrdersAsync(GetClosedOrdersRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Futures get order trades request options
        /// </summary>
        EndpointOptions<GetOrderTradesRequest> GetFuturesOrderTradesOptions { get; }
        /// <summary>
        /// Get trades for a specific futures order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetFuturesOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures user trades request options
        /// </summary>
        PaginatedEndpointOptions<GetUserTradesRequest> GetFuturesUserTradesOptions { get; }
        /// <summary>
        /// Get futures user trade records
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetFuturesUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Futures cancel order request options
        /// </summary>
        EndpointOptions<CancelOrderRequest> CancelFuturesOrderOptions { get; }
        /// <summary>
        /// Cancel a futures order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Positions request options
        /// </summary>
        EndpointOptions<GetPositionsRequest> GetPositionsOptions { get; }
        /// <summary>
        /// Get open position info
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedPosition>>> GetPositionsAsync(GetPositionsRequest request, CancellationToken ct = default);

        /// <summary>
        /// Close position order request options
        /// </summary>
        EndpointOptions<ClosePositionRequest> ClosePositionOptions { get; }
        /// <summary>
        /// Close a currently open position
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<SharedId>> ClosePositionAsync(ClosePositionRequest request, CancellationToken ct = default);
    }
}
