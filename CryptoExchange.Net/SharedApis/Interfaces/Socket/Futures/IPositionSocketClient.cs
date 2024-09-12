using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.ResponseModels;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Socket;

namespace CryptoExchange.Net.SharedApis.Interfaces.Socket.Futures
{
    public interface IPositionSocketClient : ISharedClient
    {
        SubscriptionOptions<SubscribePositionRequest> SubscribePositionOptions { get; }
        Task<ExchangeResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(SubscribePositionRequest request, Action<ExchangeEvent<IEnumerable<SharedPosition>>> handler, ApiType? apiType = null, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
