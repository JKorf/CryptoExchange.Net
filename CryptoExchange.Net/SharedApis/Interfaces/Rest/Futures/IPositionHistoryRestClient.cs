using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for getting position history
    /// </summary>
    public interface IPositionHistoryRestClient : ISharedClient
    {
        /// <summary>
        /// Position history request options
        /// </summary>
        GetPositionHistoryOptions GetPositionHistoryOptions { get; }
        /// <summary>
        /// Get position history
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="nextPageToken">The pagination request from the previous request result `NextPageRequest` property to continue pagination</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedPositionHistory[]>> GetPositionHistoryAsync(GetPositionHistoryRequest request, PageRequest? nextPageToken = null, CancellationToken ct = default);
    }
}
