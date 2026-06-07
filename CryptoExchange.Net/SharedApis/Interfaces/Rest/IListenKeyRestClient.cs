using CryptoExchange.Net.Objects;
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
        /// Start listen key request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<StartListenKeyRequest, IListenKeyRestClient> StartOptions { get; }
        /// <summary>
        /// Get the listen key which can be used for user data updates on the socket client, see <see cref="StartOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<string>> StartListenKeyAsync(StartListenKeyRequest request, CancellationToken ct = default);
        /// <summary>
        /// Keep-alive listen key request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<KeepAliveListenKeyRequest, IListenKeyRestClient> KeepAliveOptions { get; }
        /// <summary>
        /// Keep-alive the listen key, needs to be called at a regular interval (typically every 30 minutes), see <see cref="KeepAliveOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<string>> KeepAliveListenKeyAsync(KeepAliveListenKeyRequest request, CancellationToken ct = default);
        /// <summary>
        /// Stop listen key request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<StopListenKeyRequest, IListenKeyRestClient> StopOptions { get; }
        /// <summary>
        /// Stop the listen key, updates will no longer be send to the user data stream for this listen key, see <see cref="StopOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<string>> StopListenKeyAsync(StopListenKeyRequest request, CancellationToken ct = default);
    }
}
