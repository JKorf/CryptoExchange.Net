using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to user trade updates
    /// </summary>
    public interface IUserTradeSocketClient : ISharedClient
    {
        /// <summary>
        /// User trade subscription options
        /// </summary>
        EndpointOptions<SubscribeUserTradeRequest> SubscribeUserTradeOptions { get; }

        /// <summary>
        /// Subscribe to user trade updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(SubscribeUserTradeRequest request, Action<ExchangeEvent<IEnumerable<SharedUserTrade>>> handler, CancellationToken ct = default);
    }
}
