﻿using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.RequestModels;
using CryptoExchange.Net.SharedApis.SubscribeModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Socket
{
    public interface ITickersSocketClient : ISharedClient
    {
        Task<ExchangeResult<UpdateSubscription>> SubscribeToAllTickerUpdatesAsync(SharedRequest request, Action<DataEvent<IEnumerable<SharedTicker>>> handler, CancellationToken ct = default);
    }
}