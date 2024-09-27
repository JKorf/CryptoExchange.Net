using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects.Sockets;

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
        EndpointOptions<SubscribePositionRequest> SubscribePositionOptions { get; }

        /// <summary>
        /// Subscribe to user position updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(SubscribePositionRequest request, Action<ExchangeEvent<IEnumerable<SharedPosition>>> handler, CancellationToken ct = default);
    }
}
