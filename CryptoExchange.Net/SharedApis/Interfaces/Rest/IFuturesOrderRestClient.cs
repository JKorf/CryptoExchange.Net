using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces
{
    public interface IFuturesOrderRestClient : ISharedClient
    {
        Task<WebCallResult<SharedOrderId>> PlaceOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);
    }
}
