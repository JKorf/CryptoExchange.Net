using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IListenKeyRestClient : ISharedClient
    {
        EndpointOptions<StartListenKeyRequest> StartOptions { get; }
        Task<ExchangeWebResult<string>> StartListenKeyAsync(StartListenKeyRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        EndpointOptions<KeepAliveListenKeyRequest> KeepAliveOptions { get; }
        Task<ExchangeWebResult<string>> KeepAliveListenKeyAsync(KeepAliveListenKeyRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        EndpointOptions<StopListenKeyRequest> StopOptions { get; }
        Task<ExchangeWebResult<string>> StopListenKeyAsync(StopListenKeyRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
