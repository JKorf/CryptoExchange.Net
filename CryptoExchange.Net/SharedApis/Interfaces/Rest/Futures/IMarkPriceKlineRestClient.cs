using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Options.Endpoints;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Futures
{
    /// <summary>
    /// Client for getting the mark price klines for a symbol
    /// </summary>
    public interface IMarkPriceKlineRestClient : ISharedClient
    {
        /// <summary>
        /// Mark price klines request options
        /// </summary>
        GetKlinesOptions GetMarkPriceKlinesOptions { get; }
        /// <summary>
        /// Get mark price kline/candlestick data
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<IEnumerable<SharedFuturesKline>>> GetMarkPriceKlinesAsync(GetKlinesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
