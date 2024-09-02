using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IFuturesTickerRestClient : ISharedClient
    {
        EndpointOptions<GetTickerRequest> GetFuturesTickerOptions { get; }
        Task<ExchangeWebResult<SharedFuturesTicker>> GetFuturesTickerAsync(GetTickerRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        EndpointOptions GetFuturesTickersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedFuturesTicker>>> GetFuturesTickersAsync(ApiType? apiType = null, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
