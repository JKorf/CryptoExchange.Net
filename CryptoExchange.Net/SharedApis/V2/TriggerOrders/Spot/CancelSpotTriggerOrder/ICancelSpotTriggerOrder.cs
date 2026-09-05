using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling a spot trigger order on an exchange.
    /// </summary>
    public interface ICancelSpotTriggerOrder : ISharedApiCapability
    {
        /// <summary>
        /// Cancel trigger order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelSpotTriggerOrderOptions CancelSpotTriggerOrderOptions { get; }
        /// <summary>
        /// Cancel a trigger order, see <see cref="CancelSpotTriggerOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> CancelSpotTriggerOrderAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling a spot trigger order on an exchange via the REST API.
    /// </summary>
    public interface ICancelSpotTriggerOrderRest : ICancelSpotTriggerOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> CancelSpotTriggerOrderAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
