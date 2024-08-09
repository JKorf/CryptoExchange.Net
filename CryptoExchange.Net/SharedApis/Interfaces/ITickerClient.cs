using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface ITickerClient : ISharedClient
    {
        Task<WebCallResult<SharedTicker>> GetTickerAsync(TickerRequest request, CancellationToken ct = default);
        Task<WebCallResult<IEnumerable<SharedTicker>>> GetTickersAsync(SharedRequest request, CancellationToken ct = default);
    }
}
