using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to user futures order updates
    /// </summary>
    public interface IFuturesOrderSocketClient : ISharedClient
    {
        /// <summary>
        /// Futures orders subscription options
        /// </summary>
        EndpointOptions<SubscribeFuturesOrderRequest> SubscribeFuturesOrderOptions { get; }

        /// <summary>
        /// Subscribe to user futures order updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToFuturesOrderUpdatesAsync(SubscribeFuturesOrderRequest request, Action<ExchangeEvent<IEnumerable<SharedFuturesOrder>>> handler, CancellationToken ct = default);
    }
}
