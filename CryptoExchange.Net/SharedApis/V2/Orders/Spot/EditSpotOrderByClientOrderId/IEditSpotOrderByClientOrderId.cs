using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for editing an open spot order by its client order id on an exchange.
    /// </summary>
    public interface IEditSpotOrderByClientOrderId : ISharedApiCapability
    {
        /// <summary>
        /// Spot edit order by client order id request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EditSpotOrderByClientOrderIdOptions EditSpotOrderByClientOrderIdOptions { get; }
        /// <summary>
        /// Edit a spot order using client order id, see <see cref="EditSpotOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> EditSpotOrderByClientOrderIdAsync(EditOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing an open spot order by its client order id on an exchange via the REST API.
    /// </summary>
    public interface IEditSpotOrderByClientOrderIdRest : IEditSpotOrderByClientOrderId, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> EditSpotOrderByClientOrderIdAsync(EditOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for editing an open Spot order by its client order id on an exchange via the WebSocket API.
    /// </summary>
    public interface IEditSpotOrderByClientOrderIdSocket : IEditSpotOrderByClientOrderId, ISharedSocket
    {
        /// <inheritdoc />
        new Task<QueryResult<SharedId>> EditSpotOrderByClientOrderIdAsync(EditOrderRequest request, CancellationToken ct = default);
    }
}
