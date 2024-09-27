using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting kline/candlestick data
    /// </summary>
    public interface IKlineRestClient : ISharedClient
    {
        /// <summary>
        /// Kline request options
        /// </summary>
        GetKlinesOptions GetKlinesOptions { get; }

        /// <summary>
        /// Get kline/candlestick data
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination token from the previous request to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<IEnumerable<SharedKline>>> GetKlinesAsync(GetKlinesRequest request, INextPageToken? nextPageToken = null, CancellationToken ct = default);
    }
}
