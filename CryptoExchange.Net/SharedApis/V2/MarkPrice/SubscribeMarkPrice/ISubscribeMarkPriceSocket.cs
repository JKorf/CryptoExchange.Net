using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for subscribing to user mark price updates
    /// </summary>
    public interface ISubscribeMarkPriceSocket : ISharedSubscription
    {
        /// <summary>
        /// Mark price subscription options
        /// </summary>
        SubscribeMarkPriceOptions SubscribeMarkPriceOptions { get; }

        /// <summary>
        /// Subscribe to user mark price updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<WebSocketResult<UpdateSubscription>> SubscribeToMarkPriceUpdatesAsync(SubscribeMarkPriceRequest request, Action<DataEvent<SharedMarkPrice>> handler, CancellationToken ct = default);
    }
}
