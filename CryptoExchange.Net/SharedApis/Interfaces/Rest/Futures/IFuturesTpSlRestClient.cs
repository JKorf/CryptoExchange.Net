using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Take profit / Stop loss client
    /// </summary>
    public interface IFuturesTpSlRestClient : ISharedClient
    {
        /// <summary>
        /// Set take profit and/or stop loss options
        /// </summary>
        EndpointOptions<SetTpSlRequest> SetFuturesTpSlOptions { get; }
        /// <summary>
        /// Set a take profit and/or stop loss for an open position
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<SharedId>> SetFuturesTpSlAsync(SetTpSlRequest request, CancellationToken ct = default);

        /// <summary>
        /// Cancel a take profit and/or stop loss options
        /// </summary>
        EndpointOptions<CancelTpSlRequest> CancelFuturesTpSlOptions { get; }
        /// <summary>
        /// Cancel an active take profit and/or stop loss for an open position
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<bool>> CancelFuturesTpSlAsync(CancelTpSlRequest request, CancellationToken ct = default);
    }
}
