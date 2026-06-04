using System;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to position updates
    /// </summary>
    public interface IPositionSocketClient : ISharedClient
    {
        /// <summary>
        /// Position subscription options
        /// </summary>
        EndpointOptions<SubscribePositionRequest, IPositionSocketClient> SubscribePositionOptions { get; }

        /// <summary>
        /// Subscribe to user position updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<WebSocketResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(SubscribePositionRequest request, Action<DataEvent<SharedPosition[]>> handler, CancellationToken ct = default);
    }
}
