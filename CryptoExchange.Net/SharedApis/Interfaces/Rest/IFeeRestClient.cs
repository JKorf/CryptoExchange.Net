using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting user trading fees
    /// </summary>
    public interface IFeeRestClient : ISharedClient
    {
        /// <summary>
        /// Fee request options
        /// </summary>
        EndpointOptions<GetFeeRequest, IFeeRestClient> GetFeeOptions { get; }

        /// <summary>
        /// Get trading fees for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFee>> GetFeesAsync(GetFeeRequest request, CancellationToken ct = default);
    }
}
