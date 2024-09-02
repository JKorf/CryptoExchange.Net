using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Spot
{
    public interface ISpotSymbolRestClient : ISharedClient
    {
        EndpointOptions GetSpotSymbolsOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedSpotSymbol>>> GetSpotSymbolsAsync(ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
