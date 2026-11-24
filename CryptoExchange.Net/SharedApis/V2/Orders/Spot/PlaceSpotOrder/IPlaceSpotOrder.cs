using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for placing a spot order on an exchange.
    /// </summary>
    /// <see cref="ISpotOrderPlacementMetadata"/>
    public interface IPlaceSpotOrderOperation : ISpotOrderPlacementMetadata
    {
        /// <summary>
        /// Spot place order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and
        /// <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the
        /// request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the
        /// request object.
        /// </summary>
        PlaceSpotOrderOptions PlaceSpotOrderOptions { get; }

        /// <summary>
        /// Place a new spot order, see <see cref="PlaceSpotOrderOptions"/> for request options and exchange specific
        /// required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<IExchangeCallResult<SharedId>> PlaceSpotOrderAsync(
            PlaceSpotOrderRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing a spot order on an exchange.
    /// </summary>
    /// <see cref="IPlaceSpotOrderOperation"/>
    public interface IPlaceSpotOrder : IPlaceSpotOrderOperation, ISharedApiCapability
    {
    }

    /// <summary>
    /// Operation for placing a spot order on an exchange via the REST API.
    /// </summary>
    public interface IPlaceSpotOrderRest : IPlaceSpotOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> PlaceSpotOrderAsync(
            PlaceSpotOrderRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for placing a spot order on an exchange via the WebSocket API.
    /// </summary>
    public interface IPlaceSpotOrderSocket : IPlaceSpotOrder, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> PlaceSpotOrderAsync(
            PlaceSpotOrderRequest request,
            CancellationToken ct = default);
    }
}
