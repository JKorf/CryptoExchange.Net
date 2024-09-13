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

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Futures
{
    public interface IMarkPriceKlineRestClient : ISharedClient
    {
        GetKlinesOptions GetMarkPriceKlinesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedMarkKline>>> GetMarkPriceKlinesAsync(GetKlinesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
