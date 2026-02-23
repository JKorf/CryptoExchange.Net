using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for getting the index price klines for a symbol
    /// </summary>
    public interface IIndexPriceKlineRestClient : ISharedClient
    {
        /// <summary>
        /// Index price klines request options
        /// </summary>
        GetKlinesOptions GetIndexPriceKlinesOptions { get; }
        /// <summary>
        /// Get index price kline/candlestick data
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFuturesKline[]>> GetIndexPriceKlinesAsync(GetKlinesRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
