using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving leverage information for a symbol on an exchange.
    /// </summary>
    public interface IGetLeverage : ISharedApiCapability
    {
        /// <summary>
        /// Leverage request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetLeverageOptions GetLeverageOptions { get; }
        /// <summary>
        /// Get the current leverage setting for a symbol, see <see cref="GetLeverageOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedLeverage>> GetLeverageAsync(GetLeverageRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving leverage information for a symbol on an exchange via the REST API.
    /// </summary>
    public interface IGetLeverageRest : IGetLeverage, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedLeverage>> GetLeverageAsync(GetLeverageRequest request, CancellationToken ct = default);
    }
}
