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

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IFuturesOrderRestClient : ISharedClient
    {
        PlaceFuturesOrderOptions PlaceFuturesOrderOptions { get; }
        Task<ExchangeWebResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

        EndpointOptions<GetOrderRequest> GetFuturesOrderOptions { get; }
        Task<ExchangeWebResult<SharedFuturesOrder>> GetFuturesOrderAsync(GetOrderRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

        EndpointOptions<GetOpenOrdersRequest> GetOpenFuturesOrdersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedFuturesOrder>>> GetOpenFuturesOrdersAsync(GetOpenOrdersRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

        PaginatedEndpointOptions<GetClosedOrdersRequest> GetClosedFuturesOrdersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedFuturesOrder>>> GetClosedFuturesOrdersAsync(GetClosedOrdersRequest request, INextPageToken? nextPageToken = null, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

        EndpointOptions<GetOrderTradesRequest> GetFuturesOrderTradesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetFuturesOrderTradesAsync(GetOrderTradesRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

        PaginatedEndpointOptions<GetUserTradesRequest> GetFuturesUserTradesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetFuturesUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken = null, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

        EndpointOptions<CancelOrderRequest> CancelFuturesOrderOptions { get; }
        Task<ExchangeWebResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
