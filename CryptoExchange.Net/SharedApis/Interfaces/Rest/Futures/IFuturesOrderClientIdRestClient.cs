using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing futures orders using a client order id
    /// </summary>
    public interface IFuturesOrderClientIdRestClient : ISharedClient
    {
        /// <summary>
        /// Futures get order by client order id request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesOrderByClientOrderIdOptions GetFuturesOrderByClientOrderIdOptions { get; }

        /// <summary>
        /// Get info on a specific futures order using a client order id, see <see cref="GetFuturesOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesOrder>> GetFuturesOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct = default);

        /// <summary>
        /// Futures cancel order by client order id request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelFuturesOrderByClientOrderIdOptions CancelFuturesOrderByClientOrderIdOptions { get; }
        /// <summary>
        /// Cancel a futures order using client order id, see <see cref="CancelFuturesOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
