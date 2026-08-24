using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    public interface ISetPositionModeRestClient : ISharedClient
    {
        /// <summary>
        /// How the exchange handles setting the position mode
        /// </summary>
        SharedPositionModeSelection PositionModeSettingType { get; }

        /// <summary>
        /// Position mode set request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        SetPositionModeOptions SetPositionModeOptions { get; }
        /// <summary>
        /// Set the position mode to a new value, see <see cref="SetPositionModeOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedPositionModeResult>> SetPositionModeAsync(SetPositionModeRequest request, CancellationToken ct = default);
    }
}
