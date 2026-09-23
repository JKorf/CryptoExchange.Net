using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for subscribing to user Index price updates
    /// </summary>
    public interface ISubscribeIndexPriceSocket : ISharedSubscription
    {
        /// <summary>
        /// Index price subscription options
        /// </summary>
        SubscribeIndexPriceOptions SubscribeIndexPriceOptions { get; }

        /// <summary>
        /// Subscribe to user Index price updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<WebSocketResult<UpdateSubscription>> SubscribeToIndexPriceUpdatesAsync(SubscribeIndexPriceRequest request, Action<DataEvent<SharedIndexPrice>> handler, CancellationToken ct = default);
    }
}
