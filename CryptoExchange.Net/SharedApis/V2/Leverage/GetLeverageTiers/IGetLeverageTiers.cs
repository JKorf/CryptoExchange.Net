using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving leverage tier information for a symbol on an exchange.
    /// </summary>
    public interface IGetLeverageTiers : ISharedApiCapability
    {
        /// <summary>
        /// Leverage tier request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetLeverageTiersOptions GetLeverageTiersOptions { get; }
        /// <summary>
        /// Get the leverage tiers for a symbol, see <see cref="GetLeverageTiersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedLeverageTier[]>> GetLeverageTiersAsync(GetLeverageTiersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving leverage tier information for a symbol on an exchange via the REST API.
    /// </summary>
    public interface IGetLeverageTiersRest : IGetLeverageTiers, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedLeverageTier[]>> GetLeverageTiersAsync(GetLeverageTiersRequest request, CancellationToken ct = default);
    }
}
