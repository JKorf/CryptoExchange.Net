using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IRecentTradeRestClient : ISharedClient
    {
        Task<ExchangeWebResult<IEnumerable<SharedTrade>>> GetRecentTradesAsync(GetRecentTradesRequest request, CancellationToken ct = default);
    }
}
