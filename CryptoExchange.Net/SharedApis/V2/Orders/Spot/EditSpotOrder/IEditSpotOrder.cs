using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for editing an open spot order on an exchange.
    /// </summary>
    public interface IEditSpotOrder : ISharedApiCapability
    {
        /// <summary>
        /// Spot edit order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EditSpotOrderOptions EditSpotOrderOptions { get; }
        
        /// <summary>
        /// Edit an existing spot order, see <see cref="EditSpotOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> EditSpotOrderAsync(
            EditSpotOrderRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing a spot order on an exchange via the REST API.
    /// </summary>
    public interface IEditSpotOrderRest : IEditSpotOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> EditSpotOrderAsync(EditSpotOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing a spot order on an exchange via the WebSocket API.
    /// </summary>
    public interface IEditSpotOrderSocket : IEditSpotOrder, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> EditSpotOrderAsync(EditSpotOrderRequest request, CancellationToken ct = default);
    }
}
