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

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Futures
{
    public interface IPositionRestClient
    {
        EndpointOptions<GetPositionsRequest> GetPositionsOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedPosition>>> GetPositionsAsync(GetPositionsRequest request, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

    }
}
