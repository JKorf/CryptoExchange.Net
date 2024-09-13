using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Futures
{
    public interface IFuturesOrderRestClient : ISharedClient
    {
        PlaceFuturesOrderOptions PlaceFuturesOrderOptions { get; }
        Task<ExchangeWebResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);

        EndpointOptions<GetOrderRequest> GetFuturesOrderOptions { get; }
        Task<ExchangeWebResult<SharedFuturesOrder>> GetFuturesOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        EndpointOptions<GetOpenOrdersRequest> GetOpenFuturesOrdersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedFuturesOrder>>> GetOpenFuturesOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);

        PaginatedEndpointOptions<GetClosedOrdersRequest> GetClosedFuturesOrdersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedFuturesOrder>>> GetClosedFuturesOrdersAsync(GetClosedOrdersRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        EndpointOptions<GetOrderTradesRequest> GetFuturesOrderTradesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetFuturesOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

        PaginatedEndpointOptions<GetUserTradesRequest> GetFuturesUserTradesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetFuturesUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        EndpointOptions<CancelOrderRequest> CancelFuturesOrderOptions { get; }
        Task<ExchangeWebResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

        EndpointOptions<GetPositionsRequest> GetPositionsOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedPosition>>> GetPositionsAsync(GetPositionsRequest request, CancellationToken ct = default);

        EndpointOptions<ClosePositionRequest> ClosePositionOptions { get; }
        Task<ExchangeWebResult<SharedId>> ClosePositionAsync(ClosePositionRequest request, CancellationToken ct = default);
    }
}
