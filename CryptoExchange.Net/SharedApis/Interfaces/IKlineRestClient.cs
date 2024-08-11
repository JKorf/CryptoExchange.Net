using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IKlineRestClient: ISharedClient
    {
        Task<WebCallResult<IEnumerable<SharedKline>>> GetKlinesAsync(GetKlinesRequest request, CancellationToken ct = default);
    }
}
