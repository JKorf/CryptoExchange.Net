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

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Spot
{
    public interface ISpotTickerRestClient : ISharedClient
    {
        EndpointOptions<GetTickerRequest> GetSpotTickerOptions { get; }
        Task<ExchangeWebResult<SharedSpotTicker>> GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct = default);
        EndpointOptions<GetTickersRequest> GetSpotTickersOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedSpotTicker>>> GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }
}
