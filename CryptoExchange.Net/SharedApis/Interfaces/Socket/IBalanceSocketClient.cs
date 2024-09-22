using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Options.Endpoints;
using CryptoExchange.Net.SharedApis.Models.Options.Subscriptions;
using CryptoExchange.Net.SharedApis.Models.Socket;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Socket
{
    /// <summary>
    /// Client for subscribing to user balance updates
    /// </summary>
    public interface IBalanceSocketClient : ISharedClient
    {
        /// <summary>
        /// Balance subscription options
        /// </summary>
        EndpointOptions<SubscribeBalancesRequest> SubscribeBalanceOptions { get; }

        /// <summary>
        /// Subscribe to user balance updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(SubscribeBalancesRequest request, Action<ExchangeEvent<IEnumerable<SharedBalance>>> handler, CancellationToken ct = default);
    }
}
