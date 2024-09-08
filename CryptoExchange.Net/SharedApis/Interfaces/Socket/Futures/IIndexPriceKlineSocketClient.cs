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
    public interface IIndexPriceKlineSocketClient : ISharedClient
    {
        SubscribeKlineOptions SubscribeIndexPriceKlineOptions { get; }
        Task<ExchangeResult<UpdateSubscription>> SubscribeToIndexPriceKlineUpdatesAsync(SubscribeKlineRequest request, Action<ExchangeEvent<SharedMarkKline>> handler, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
