using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing futures orders using a client order id
    /// </summary>
    public interface IFuturesOrderClientIdRestClient : ISharedClient
    {
        /// <summary>
        /// Futures get order by client order id request options
        /// </summary>
        EndpointOptions<GetOrderRequest> GetFuturesOrderByClientOrderIdOptions { get; }

        /// <summary>
        /// Get info on a specific futures order using a client order id
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFuturesOrder>> GetFuturesOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures cancel order by client order id request options
        /// </summary>
        EndpointOptions<CancelOrderRequest> CancelFuturesOrderByClientOrderIdOptions { get; }
        /// <summary>
        /// Cancel a futures order using client order id
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
