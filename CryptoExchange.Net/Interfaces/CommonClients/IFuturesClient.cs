using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces.CommonClients;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Interfaces.CommonClients
{
    /// <summary>
    /// Common futures endpoints
    /// </summary>
    public interface IFuturesClient : IBaseRestClient
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
        /// <param name="leverage">[Optional] Leverage for this order. This is needed for some exchanges. For exchanges where this is not needed this parameter is ignored (and should be set before hand)</param>
        /// <param name="clientOrderId">[Optional] Client specified id for this order</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns>The id of the resulting order</returns>
        Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price = null, int? leverage = null, string? accountId = null, string? clientOrderId = null, CancellationToken ct = default);

        /// <summary>
        /// Get position
        /// </summary>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Position>>> GetPositionsAsync(CancellationToken ct = default);
    }
}
