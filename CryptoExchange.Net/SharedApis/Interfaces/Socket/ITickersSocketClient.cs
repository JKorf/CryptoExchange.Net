using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
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
        SubscriptionOptions SubscribeAllTickersOptions { get; }
        Task<ExchangeResult<UpdateSubscription>> SubscribeToAllTickersUpdatesAsync(ApiType apiType, Action<ExchangeEvent<IEnumerable<SharedSpotTicker>>> handler, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
