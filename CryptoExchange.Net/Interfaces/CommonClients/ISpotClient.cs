using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces.CommonClients;
using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces.CommonClients
{
    /// <summary>
    /// Common spot endpoints
    /// </summary>
    public interface ISpotClient: IBaseRestClient
    {
        /// <summary>
        /// Place an order
        /// </summary>
        /// <param name="symbol">The symbol the order is for</param>
        /// <param name="side">The side of the order</param>
        /// <param name="type">The type of the order</param>
        /// <param name="quantity">The quantity of the order</param>
        /// <param name="price">The price of the order, only for limit orders</param>
        /// <param name="accountId">[Optional] The account id to place the order on, required for some exchanges, ignored otherwise</param>
        /// <param name="clientOrderId">[Optional] Client specified id for this order</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns>The id of the resulting order</returns>
        Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price = null, string? accountId = null, string? clientOrderId = null, CancellationToken ct = default);
    }
}
