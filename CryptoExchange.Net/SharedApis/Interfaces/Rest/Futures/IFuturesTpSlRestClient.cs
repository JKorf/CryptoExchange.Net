using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Take profit / Stop loss client
    /// </summary>
    public interface IFuturesTpSlRestClient : ISharedClient
    {
        /// <summary>
        /// Set take profit and/or stop loss options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<SetTpSlRequest, IFuturesTpSlRestClient> SetFuturesTpSlOptions { get; }
        /// <summary>
        /// Set a take profit and/or stop loss for an open position, see <see cref="SetFuturesTpSlOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<SharedId>> SetFuturesTpSlAsync(SetTpSlRequest request, CancellationToken ct = default);

        /// <summary>
        /// Cancel a take profit and/or stop loss options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<CancelTpSlRequest, IFuturesTpSlRestClient> CancelFuturesTpSlOptions { get; }
        /// <summary>
        /// Cancel an active take profit and/or stop loss for an open position, see <see cref="CancelFuturesTpSlOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<bool>> CancelFuturesTpSlAsync(CancelTpSlRequest request, CancellationToken ct = default);
    }
}
