using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IFuturesSymbolClient : ISharedClient
    {
        Task<WebCallResult<IEnumerable<SharedFuturesSymbol>>> GetSymbolsAsync(SharedRequest request, CancellationToken ct = default);
    }
}
