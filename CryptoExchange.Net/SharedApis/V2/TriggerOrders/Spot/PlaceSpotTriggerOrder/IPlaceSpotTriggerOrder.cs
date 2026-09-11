using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for placing a spot trigger order on an exchange.
    /// </summary>
    public interface IPlaceSpotTriggerOrder : ISharedApiCapability
    {
        /// <summary>
        /// Place spot trigger order options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceSpotTriggerOrderOptions PlaceSpotTriggerOrderOptions { get; }

        /// <summary>
        /// Place a new trigger order, see <see cref="PlaceSpotTriggerOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedId>> PlaceSpotTriggerOrderAsync(PlaceSpotTriggerOrderRequest request, CancellationToken ct = default);

    }

    /// <summary>
    /// Operation for placing a spot trigger order on an exchange via the REST API.
    /// </summary>
    public interface IPlaceSpotTriggerOrderRest : IPlaceSpotTriggerOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> PlaceSpotTriggerOrderAsync(PlaceSpotTriggerOrderRequest request, CancellationToken ct = default);

    }
}
