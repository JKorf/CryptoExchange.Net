﻿using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Options.Endpoints;
using CryptoExchange.Net.SharedApis.Models.Options.Subscriptions;
using CryptoExchange.Net.SharedApis.Models.Socket;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Socket.Spot
{
    /// <summary>
    /// Client for subscribing to user spot order updates
    /// </summary>
    public interface ISpotOrderSocketClient : ISharedClient
    {
        /// <summary>
        /// Spot orders subscription options
        /// </summary>
        EndpointOptions<SubscribeSpotOrderRequest> SubscribeSpotOrderOptions { get; }

        /// <summary>
        /// Subscribe to user spot order updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToSpotOrderUpdatesAsync(SubscribeSpotOrderRequest request, Action<ExchangeEvent<IEnumerable<SharedSpotOrder>>> handler, CancellationToken ct = default);
    }
}