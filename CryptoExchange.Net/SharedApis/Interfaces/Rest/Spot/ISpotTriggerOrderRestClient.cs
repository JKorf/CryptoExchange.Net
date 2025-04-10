using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing trigger orders
    /// </summary>
    public interface ISpotTriggerOrderRestClient : ISharedClient
    {
        /// <summary>
        /// Place spot trigger order options
        /// </summary>
        PlaceSpotTriggerOrderOptions PlaceSpotTriggerOrderOptions { get; }

        /// <summary>
        /// Place a new trigger order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<SharedId>> PlaceSpotTriggerOrderAsync(PlaceSpotTriggerOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Get trigger order request options
        /// </summary>
        EndpointOptions<GetOrderRequest> GetSpotTriggerOrderOptions { get; }
        /// <summary>
        /// Get info on a specific trigger order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedSpotTriggerOrder>> GetSpotTriggerOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Cancel trigger order request options
        /// </summary>
        EndpointOptions<CancelOrderRequest> CancelSpotTriggerOrderOptions { get; }
        /// <summary>
        /// Cancel a trigger order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> CancelSpotTriggerOrderAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
