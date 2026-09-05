using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving user trading fees from an exchange.
    /// </summary>
    public interface IGetFees : ISharedApiCapability
    {
        /// <summary>
        /// Fee request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFeeOptions GetFeeOptions { get; }

        /// <summary>
        /// Get trading fees for a symbol, see <see cref="GetFeeOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFee>> GetFeesAsync(GetFeeRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving user trading fees from an exchange via the REST API.
    /// </summary>
    public interface IGetFeesRest : IGetFees, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFee>> GetFeesAsync(GetFeeRequest request, CancellationToken ct = default);
    }
}
