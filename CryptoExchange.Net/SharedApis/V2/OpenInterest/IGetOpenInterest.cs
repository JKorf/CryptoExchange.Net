using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving open interest data from an exchange.
    /// </summary>
    public interface IGetOpenInterest : ISharedApiCapability
    {
        /// <summary>
        /// Open interest request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetOpenInterestOptions GetOpenInterestOptions { get; }
        /// <summary>
        /// Get the open interest for a symbol, see <see cref="GetOpenInterestOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedOpenInterest>> GetOpenInterestAsync(GetOpenInterestRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving open interest data from an exchange via the REST API.
    /// </summary>
    public interface IGetOpenInterestRest : IGetOpenInterest, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedOpenInterest>> GetOpenInterestAsync(GetOpenInterestRequest request, CancellationToken ct = default);
    }
}
