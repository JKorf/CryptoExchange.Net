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
    public interface IFuturesTriggerOrderRestClient : ISharedClient
    {
        ///// <summary>
        ///// Trade history request options
        ///// </summary>
        //GetTradeHistoryOptions GetTradeHistoryOptions { get; }

        /// <summary>
        /// Place a new trigger order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<SharedId>> PlaceFuturesTriggerOrderAsync(PlaceFuturesTriggerOrderRequest request, CancellationToken ct = default);


        /// <summary>
        /// Get trigger order request options
        /// </summary>
        EndpointOptions<GetOrderRequest> GetFuturesTriggerOrderOptions { get; }
        /// <summary>
        /// Get info on a specific trigger order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFuturesTriggerOrder>> GetFuturesTriggerOrderAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Cancel trigger order request options
        /// </summary>
        EndpointOptions<CancelOrderRequest> CancelFuturesTriggerOrderOptions { get; }
        /// <summary>
        /// Cancel a trigger order
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> CancelFuturesTriggerOrderAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
