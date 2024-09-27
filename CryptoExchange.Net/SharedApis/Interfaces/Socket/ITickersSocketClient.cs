using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to ticker updates for all symbols
    /// </summary>
    public interface ITickersSocketClient : ISharedClient
    {
        /// <summary>
        /// Tickers subscription options
        /// </summary>
        EndpointOptions<SubscribeAllTickersRequest> SubscribeAllTickersOptions { get; }

        /// <summary>
        /// Subscribe to tickers updates for all symbols
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToAllTickersUpdatesAsync(SubscribeAllTickersRequest request, Action<ExchangeEvent<IEnumerable<SharedSpotTicker>>> handler, CancellationToken ct = default);
    }
}
