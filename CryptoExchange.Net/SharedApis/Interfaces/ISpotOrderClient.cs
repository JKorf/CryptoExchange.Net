using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface ISpotOrderClient
    {
        Task<WebCallResult<SharedOrderId>> PlaceOrderAsync(SpotPlaceOrderRequest request, CancellationToken ct = default);
        Task<WebCallResult<SharedSpotOrder>> GetOrderAsync(SpotGetOrderRequest request, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<SharedSpotOrder>>> GetOpenOrdersAsync(SpotOpenOrderRequest request, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<SharedSpotOrder>>> GetClosedOrdersAsync(SpotClosedOrderRequest request, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<SharedUserTrade>>> GetUserTradesAsync(UserTradeRequest request, CancellationToken ct = default);
        Task<WebCallResult<SharedOrderId>> CancelOrderAsync(SpotCancelOrderRequest request, CancellationToken ct = default);

    }
}
