using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface ITickerRestClient : ISharedClient
    {
        Task<ExchangeWebResult<SharedTicker>> GetTickerAsync(GetTickerRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        Task<ExchangeWebResult<IEnumerable<SharedTicker>>> GetTickersAsync(ApiType? apiType, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
