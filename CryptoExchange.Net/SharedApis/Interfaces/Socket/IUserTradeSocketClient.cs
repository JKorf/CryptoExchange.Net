using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Socket;
using CryptoExchange.Net.SharedApis.RequestModels;
using CryptoExchange.Net.SharedApis.ResponseModels;
using CryptoExchange.Net.SharedApis.SubscribeModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Socket
{
    public interface IUserTradeSocketClient : ISharedClient
    {
        SubscriptionOptions SubscribeUserTradeOptions { get; }
        Task<ExchangeResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(ApiType apiType, Action<ExchangeEvent<IEnumerable<SharedUserTrade>>> handler, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
