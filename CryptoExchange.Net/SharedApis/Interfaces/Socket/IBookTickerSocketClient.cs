using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to book ticker updates for a symbol
    /// </summary>
    public interface IBookTickerSocketClient : ISharedClient
    {
        /// <summary>
        /// Book ticker subscription options
        /// </summary>
        EndpointOptions<SubscribeBookTickerRequest> SubscribeBookTickerOptions { get; }

        /// <summary>
        /// Subscribe to book ticker (best ask/bid) updates for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToBookTickerUpdatesAsync(SubscribeBookTickerRequest request, Action<ExchangeEvent<SharedBookTicker>> handler, CancellationToken ct = default);
    }
}
