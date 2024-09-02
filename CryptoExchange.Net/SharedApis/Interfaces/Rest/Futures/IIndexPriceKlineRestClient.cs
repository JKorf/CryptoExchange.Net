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
    public interface IIndexPriceKlineRestClient : ISharedClient
    {
        GetKlinesOptions GetIndexPriceKlinesOptions { get; }
        Task<ExchangeWebResult<IEnumerable<SharedMarkKline>>> GetIndexPriceKlinesAsync(GetKlinesRequest request, INextPageToken? nextPageToken = null, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
