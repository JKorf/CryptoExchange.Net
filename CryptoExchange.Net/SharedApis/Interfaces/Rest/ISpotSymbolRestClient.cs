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
    public interface ISpotSymbolRestClient : ISharedClient
    {
        Task<ExchangeWebResult<IEnumerable<SharedSpotSymbol>>> GetSpotSymbolsAsync(CancellationToken ct = default);
    }
}
