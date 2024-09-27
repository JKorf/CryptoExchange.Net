using System.Collections.Generic;
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
        /// Supported order types
        /// </summary>
        IEnumerable<SharedOrderType> SpotSupportedOrderTypes { get; }
        /// <summary>
        /// Supported time in force
        /// </summary>
        IEnumerable<SharedTimeInForce> SpotSupportedTimeInForce { get; }
        /// <summary>
        /// Quantity types support
        /// </summary>
        SharedQuantitySupport SpotSupportedOrderQuantity { get; }

        /// <summary>
        /// Spot place order request options
        /// </summary>
        PlaceSpotOrderOptions PlaceSpotOrderOptions { get; }
        /// <summary>
        /// Place a new spot order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get order request options
        /// </summary>
        EndpointOptions<GetOrderRequest> GetSpotOrderOptions { get; }
        /// <summary>
        /// Get info on a specific spot order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedSpotOrder>> GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get open orders request options
        /// </summary>
        EndpointOptions<GetOpenOrdersRequest> GetOpenSpotOrdersOptions { get; }
        /// <summary>
        /// Get info on a open spot orders
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot get closed orders request options
        /// </summary>
        PaginatedEndpointOptions<GetClosedOrdersRequest> GetClosedSpotOrdersOptions { get; }
        /// <summary>
        /// Get info on closed spot orders
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Spot get order trades request options
        /// </summary>
        EndpointOptions<GetOrderTradesRequest> GetSpotOrderTradesOptions { get; }
        /// <summary>
        /// Get trades for a specific spot order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot user trades request options
        /// </summary>
        PaginatedEndpointOptions<GetUserTradesRequest> GetSpotUserTradesOptions { get; }
        /// <summary>
        /// Get spot user trade records
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetSpotUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        /// <summary>
        /// Spot cancel order request options
        /// </summary>
        EndpointOptions<CancelOrderRequest> CancelSpotOrderOptions { get; }
        /// <summary>
        /// Cancel a spot order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }
}
