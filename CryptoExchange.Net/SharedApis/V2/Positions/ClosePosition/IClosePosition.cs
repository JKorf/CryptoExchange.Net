using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for closing an open position on an exchange.
    /// </summary>
    public interface IClosePosition : ISharedApiCapability
    {
        /// <summary>
        /// Close position order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        ClosePositionOptions ClosePositionOptions { get; }
        /// <summary>
        /// Close a currently open position, see <see cref="ClosePositionOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedId>> ClosePositionAsync(ClosePositionRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for closing an open position on an exchange via the REST API.
    /// </summary>
    public interface IClosePositionRest : IClosePosition, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> ClosePositionAsync(ClosePositionRequest request, CancellationToken ct = default);
    }
}
