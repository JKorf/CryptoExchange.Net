using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Interfaces.CommonClients
{
    /// <summary>
    /// DEPRECATED; use <see cref="SharedApis.ISharedClient" /> instead for common/shared functionality. See <see href="https://jkorf.github.io/CryptoExchange.Net/docs/index.html#shared" /> for more info.
    /// </summary>
    public interface IFuturesClient : IBaseRestClient
    {
        /// <summary>
        /// DEPRECATED; use <see cref="SharedApis.ISharedClient" /> instead for common/shared functionality. See <see href="https://jkorf.github.io/CryptoExchange.Net/docs/index.html#shared" /> for more info.
        /// </summary>
        Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price = null, int? leverage = null, string? accountId = null, string? clientOrderId = null, CancellationToken ct = default);

        /// <summary>
        /// DEPRECATED; use <see cref="SharedApis.ISharedClient" /> instead for common/shared functionality. See <see href="https://jkorf.github.io/CryptoExchange.Net/docs/index.html#shared" /> for more info.
        /// </summary>
        Task<WebCallResult<IEnumerable<Position>>> GetPositionsAsync(CancellationToken ct = default);
    }
}
