using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for subscribing to ticker updates for all symbols
    /// </summary>
    public interface ISubscribeAllTickersSocket : ISharedSubscription
    {
        /// <summary>
        /// Tickers subscription options
        /// </summary>
        SubscribeTickersOptions SubscribeAllTickersOptions { get; }

        /// <summary>
        /// Subscribe to tickers updates for all symbols
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<WebSocketResult<UpdateSubscription>> SubscribeToAllTickersUpdatesAsync(SubscribeAllTickersRequest request, Action<DataEvent<SharedTicker[]>> handler, CancellationToken ct = default);
    }
}
