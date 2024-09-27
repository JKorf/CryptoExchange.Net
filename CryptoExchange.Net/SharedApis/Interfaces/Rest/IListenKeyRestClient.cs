using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing the listen key for user stream updates
    /// </summary>
    public interface IListenKeyRestClient : ISharedClient
    {
        /// <summary>
        /// Start listen key request options
        /// </summary>
        EndpointOptions<StartListenKeyRequest> StartOptions { get; }
        /// <summary>
        /// Get the listen key which can be used for user data updates on the socket client
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<string>> StartListenKeyAsync(StartListenKeyRequest request, CancellationToken ct = default);
        /// <summary>
        /// Keep-alive listen key request options
        /// </summary>
        EndpointOptions<KeepAliveListenKeyRequest> KeepAliveOptions { get; }
        /// <summary>
        /// Keep-alive the listen key, needs to be called at a regular interval (typically every 30 minutes)
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<string>> KeepAliveListenKeyAsync(KeepAliveListenKeyRequest request, CancellationToken ct = default);
        /// <summary>
        /// Stop listen key request options
        /// </summary>
        EndpointOptions<StopListenKeyRequest> StopOptions { get; }
        /// <summary>
        /// Stop the listen key, updates will no longer be send to the user data stream for this listen key
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<string>> StopListenKeyAsync(StopListenKeyRequest request, CancellationToken ct = default);
    }
}
