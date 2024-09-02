using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.EndpointOptions;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Futures
{
    public interface ILeverageRestClient
    {
        EndpointOptions<GetLeverageRequest> GetLeverageOptions { get; }
        Task<ExchangeWebResult<SharedLeverage>> GetLeverageAsync(GetLeverageRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        SetLeverageOptions SetLeverageOptions { get; }
        Task<ExchangeWebResult<SharedLeverage>> SetLeverageAsync(SetLeverageRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

    }
}
