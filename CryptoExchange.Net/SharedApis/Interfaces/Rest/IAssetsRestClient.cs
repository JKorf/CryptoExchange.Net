using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.RequestModels;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IAssetsRestClient : ISharedClient
    {
        EndpointOptions<GetAssetRequest> GetAssetOptions { get; }
        Task<ExchangeWebResult<SharedAsset>> GetAssetAsync(GetAssetRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
        EndpointOptions GetAssetsOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedAsset>>> GetAssetsAsync(ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
