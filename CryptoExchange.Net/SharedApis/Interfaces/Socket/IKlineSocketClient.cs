using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis.SubscribeModels;
using CryptoExchange.Net.SharedApis.ResponseModels;
using CryptoExchange.Net.SharedApis.RequestModels;
using CryptoExchange.Net.SharedApis.Models.Socket;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;

namespace CryptoExchange.Net.SharedApis.Interfaces.Socket
{
    public interface IKlineSocketClient : ISharedClient
    {
        SubscribeKlineOptions SubscribeKlineOptions { get; }
        Task<ExchangeResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(SubscribeKlineRequest request, Action<ExchangeEvent<SharedKline>> handler, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
