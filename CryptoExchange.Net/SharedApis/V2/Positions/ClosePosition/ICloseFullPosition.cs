using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for fully closing an open position on an exchange.
    /// </summary>
    public interface ICloseFullPosition : ISharedApiCapability
    {
        /// <summary>
        /// Close position order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CloseFullPositionOptions CloseFullPositionOptions { get; }
        /// <summary>
        /// Fully close a currently open position, see <see cref="CloseFullPositionOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedId>> CloseFullPositionAsync(CloseFullPositionRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for fully closing an open position on an exchange via the REST API.
    /// </summary>
    public interface ICloseFullPositionRest : ICloseFullPosition, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> CloseFullPositionAsync(CloseFullPositionRequest request, CancellationToken ct = default);
    }
}
