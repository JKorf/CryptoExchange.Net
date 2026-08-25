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
    public interface IGetPositionModeEndpoint : ISharedApiEndpoint
    {
        /// <summary>
        /// Position mode request options.<br />
        /// Use <see cref="EndpointOptions{TRequest, TClient}.RequiredRequestParameters"/>, <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetPositionModeOptions GetPositionModeOptions { get; }
        /// <summary>
        /// Get the current position mode setting, see <see cref="GetPositionModeOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedPositionModeResult>> GetPositionModeAsync(GetPositionModeRequest request, CancellationToken ct = default);
    }
}
