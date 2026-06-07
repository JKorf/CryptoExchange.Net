using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting to withdraw funds from the exchange
    /// </summary>
    public interface IWithdrawRestClient : ISharedClient
    {
        /// <summary>
        /// Withdraw request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        WithdrawOptions WithdrawOptions { get; }

        /// <summary>
        /// Request a withdrawal, see <see cref="WithdrawOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<HttpResult<SharedId>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default);
    }
}
