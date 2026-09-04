using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for setting the user position mode on an exchange.
    /// </summary>
    public interface ISetPositionMode : ISharedApiCapability
    {
        /// <summary>
        /// How the exchange handles setting the position mode
        /// </summary>
        SharedPositionModeSelection PositionModeSettingType { get; }

        /// <summary>
        /// Position mode set request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        SetPositionModeOptions SetPositionModeOptions { get; }
        /// <summary>
        /// Set the position mode to a new value, see <see cref="SetPositionModeOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedPositionModeResult>> SetPositionModeAsync(SetPositionModeRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for setting the user position mode on an exchange via the REST API.
    /// </summary>
    public interface ISetPositionModeRest : ISetPositionMode, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedPositionModeResult>> SetPositionModeAsync(SetPositionModeRequest request, CancellationToken ct = default);
    }
}
