using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to public trade updates for a symbol
    /// </summary>
    public interface ITradeSocketClient : ISharedClient
    {
        /// <summary>
        /// Trade subscription options
        /// </summary>
        EndpointOptions<SubscribeTradeRequest> SubscribeTradeOptions { get; }

        /// <summary>
        /// Subscribe to public trade updates for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(SubscribeTradeRequest request, Action<ExchangeEvent<IEnumerable<SharedTrade>>> handler, CancellationToken ct = default);
    }
}
