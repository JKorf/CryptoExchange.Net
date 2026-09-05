using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving a single futures ticker from an exchange.
    /// </summary>
    public interface IGetFuturesTicker : ISharedApiCapability
    {
        /// <summary>
        /// Futures get ticker request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesTickerOptions GetFuturesTickerOptions { get; }
        /// <summary>
        /// Get ticker info for a specific futures symbol, see <see cref="GetFuturesTickerOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesTicker>> GetFuturesTickerAsync(GetTickerRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving a single futures ticker from an exchange via the REST API.
    /// </summary>
    public interface IGetFuturesTickerRest : IGetFuturesTicker, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFuturesTicker>> GetFuturesTickerAsync(GetTickerRequest request, CancellationToken ct = default);
    }
}
