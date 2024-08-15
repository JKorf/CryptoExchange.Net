using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface ISpotOrderRestClient : ISharedClient
    {
        public IEnumerable<SharedOrderType> SupportedOrderType { get; }
        public IEnumerable<SharedTimeInForce> SupportedTimeInForce { get; }

        public SharedQuantitySupport OrderQuantitySupport { get; } 

        Task<ExchangeWebResult<SharedOrderId>> PlaceOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct = default);
        Task<ExchangeWebResult<SharedSpotOrder>> GetOrderAsync(GetOrderRequest request, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetOpenOrdersAsync(GetSpotOpenOrdersRequest request, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedSpotOrder>>> GetClosedOrdersAsync(GetSpotClosedOrdersRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedUserTrade>>> GetUserTradesAsync(GetUserTradesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
        Task<ExchangeWebResult<SharedOrderId>> CancelOrderAsync(CancelOrderRequest request, CancellationToken ct = default);

    }
}
