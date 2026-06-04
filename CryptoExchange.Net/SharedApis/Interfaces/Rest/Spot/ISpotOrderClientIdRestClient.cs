using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing spot orders using a client order id
    /// </summary>
    public interface ISpotOrderClientIdRestClient : ISharedClient
    {
        /// <summary>
        /// Spot get order by client order id request options
        /// </summary>
        EndpointOptions<GetOrderRequest, ISpotOrderClientIdRestClient> GetSpotOrderByClientOrderIdOptions { get; }

        /// <summary>
        /// Get info on a specific spot order using a client order id
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedSpotOrder>> GetSpotOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Spot cancel order by client order id request options
        /// </summary>
        EndpointOptions<CancelOrderRequest, ISpotOrderClientIdRestClient> CancelSpotOrderByClientOrderIdOptions { get; }
        /// <summary>
        /// Cancel a spot order using client order id
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedId>> CancelSpotOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
