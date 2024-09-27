using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to order book snapshot updates for a symbol
    /// </summary>
    public interface IOrderBookSocketClient : ISharedClient
    {
        /// <summary>
        /// Order book subscription options
        /// </summary>
        SubscribeOrderBookOptions SubscribeOrderBookOptions { get; }

        /// <summary>
        /// Subscribe to order book snapshot updates for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(SubscribeOrderBookRequest request, Action<ExchangeEvent<SharedOrderBook>> handler, CancellationToken ct = default);
    }
}
