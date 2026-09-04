using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request defintion for getting the current user position mode setting on an exchange.
    /// </summary>
    public interface IGetPositionMode : ISharedApiCapability
    {
        /// <summary>
        /// Position mode request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetPositionModeOptions GetPositionModeOptions { get; }
        /// <summary>
        /// Get the current position mode setting, see <see cref="GetPositionModeOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedPositionModeResult>> GetPositionModeAsync(GetPositionModeRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Request defintion for getting the current user position mode setting on an exchange.
    /// </summary>
    public interface IGetPositionModeRest : IGetPositionMode, ISharedRest
    {
        /// <summary>
        /// Get the current position mode setting, see <see cref="GetPositionModeOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<HttpResult<SharedPositionModeResult>> GetPositionModeAsync(GetPositionModeRequest request, CancellationToken ct = default);
    }
}
