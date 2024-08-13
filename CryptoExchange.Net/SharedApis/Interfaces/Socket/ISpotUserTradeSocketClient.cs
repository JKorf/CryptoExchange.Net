using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
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
    public interface ISpotUserTradeSocketClient : ISharedClient
    {
        Task<CallResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(SharedRequest request, Action<DataEvent<IEnumerable<SharedUserTrade>>> handler, CancellationToken ct = default);
    }
}
