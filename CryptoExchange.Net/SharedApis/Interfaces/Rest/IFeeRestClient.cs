using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting user trading fees
    /// </summary>
    public interface IFeeRestClient : ISharedClient
    {
        /// <summary>
        /// Fee request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFeeOptions GetFeeOptions { get; }

        /// <summary>
        /// Get trading fees for a symbol, see <see cref="GetFeeOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFee>> GetFeesAsync(GetFeeRequest request, CancellationToken ct = default);
    }
}
