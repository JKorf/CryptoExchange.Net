using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
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
        public IEnumerable<SharedTimeInForce> SupportedTimeInForce { get; set; }

#warning TODO
        public IEnumerable<SharedOrderType> QuoteQuantitySupport { get; } 

        Task<WebCallResult<SharedOrderId>> PlaceOrderAsync(PlaceSpotPlaceOrderRequest request, CancellationToken ct = default);
        Task<WebCallResult<SharedSpotOrder>> GetOrderAsync(GetOrderRequest request, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<SharedSpotOrder>>> GetOpenOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<SharedSpotOrder>>> GetClosedOrdersAsync(GetClosedOrdersRequest request, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<SharedUserTrade>>> GetUserTradesAsync(GetUserTradesRequest request, CancellationToken ct = default);
        Task<WebCallResult<SharedOrderId>> CancelOrderAsync(SpotCancelOrderRequest request, CancellationToken ct = default);

    }
}
