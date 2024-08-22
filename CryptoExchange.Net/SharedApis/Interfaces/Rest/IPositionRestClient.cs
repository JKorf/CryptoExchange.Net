using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
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
        Task<ExchangeWebResult<IEnumerable<SharedPosition>>> GetPositionsAsync(ApiType? apiType, ExchangeParameters? exchangeParameters = null, CancellationToken ct = default);

    }
}
