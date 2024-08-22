using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IFuturesSymbolRestClient : ISharedClient
    {
        Task<ExchangeWebResult<IEnumerable<SharedFuturesSymbol>>> GetFuturesSymbolsAsync(ApiType? apiType, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);
    }
}
