using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Options.Endpoints;
using CryptoExchange.Net.SharedApis.Models.Options.Subscriptions;
using CryptoExchange.Net.SharedApis.Models.Socket;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Socket
{
    /// <summary>
    /// Client for subscribing to ticker updates for a symbol
    /// </summary>
    public interface ITickerSocketClient : ISharedClient
    {
        /// <summary>
        /// Ticker subscription options
        /// </summary>
        EndpointOptions<SubscribeTickerRequest> SubscribeTickerOptions { get; }

        /// <summary>
        /// Subscribe to ticker updates for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(SubscribeTickerRequest request, Action<ExchangeEvent<SharedSpotTicker>> handler, CancellationToken ct = default);
    }
}
