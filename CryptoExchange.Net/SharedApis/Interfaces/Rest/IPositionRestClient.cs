using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest
{
    public interface IPositionRestClient
    {
        Task<WebCallResult<IEnumerable<SharedPosition>>> GetPositionsAsync(SharedRequest request, CancellationToken ct = default);

    }
}
