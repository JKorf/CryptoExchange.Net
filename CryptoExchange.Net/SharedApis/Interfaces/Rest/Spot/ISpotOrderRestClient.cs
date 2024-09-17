using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Spot
{
    public interface ISpotOrderRestClient : ISharedClient
    {
        SharedFeeDeductionType SpotFeeDeductionType { get; }
        SharedFeeAssetType SpotFeeAssetType { get; }

        PlaceSpotOrderOptions PlaceSpotOrderOptions { get; }
        Task<ExchangeWebResult<SharedId>> PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct = default);

        EndpointOptions<GetOrderRequest> GetSpotOrderOptions { get; }
        Task<ExchangeWebResult<SharedSpotOrder>> GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        EndpointOptions<GetOpenOrdersRequest> GetOpenSpotOrdersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);

        PaginatedEndpointOptions<GetClosedOrdersRequest> GetClosedSpotOrdersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        EndpointOptions<GetOrderTradesRequest> GetSpotOrderTradesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

        PaginatedEndpointOptions<GetUserTradesRequest> GetSpotUserTradesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetSpotUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);

        EndpointOptions<CancelOrderRequest> CancelSpotOrderOptions { get; }
        Task<ExchangeWebResult<SharedId>> CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }
}
