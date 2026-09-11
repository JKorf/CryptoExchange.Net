using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving a specific spot trigger order from an exchange.
    /// </summary>
    public interface IGetSpotTriggerOrder : ISharedApiCapability
    {
        /// <summary>
        /// Get trigger order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotTriggerOrderOptions GetSpotTriggerOrderOptions { get; }
        /// <summary>
        /// Get info on a specific trigger order, see <see cref="GetSpotTriggerOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedSpotTriggerOrder>> GetSpotTriggerOrderAsync(GetOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving a specific spot trigger order from an exchange via the REST API.
    /// </summary>
    public interface IGetSpotTriggerOrderRest : IGetSpotTriggerOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedSpotTriggerOrder>> GetSpotTriggerOrderAsync(GetOrderRequest request, CancellationToken ct = default);
    }
}
