using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for transferring funds between account types
    /// </summary>
    public interface ITransferRestClient : ISharedClient
    {
        /// <summary>
        /// Transfer request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        TransferOptions TransferOptions { get; }

        /// <summary>
        /// Transfer funds between account types, see <see cref="TransferOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedId>> TransferAsync(TransferRequest request, CancellationToken ct = default);
    }
}
