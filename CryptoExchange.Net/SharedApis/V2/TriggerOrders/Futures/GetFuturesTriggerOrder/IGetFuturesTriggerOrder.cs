using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for getting a specific futures trigger order from the exchange.
    /// </summary>
    public interface IGetFuturesTriggerOrder : ISharedApiCapability
    {
        /// <summary>
        /// Get trigger order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesTriggerOrderOptions GetFuturesTriggerOrderOptions { get; }
        /// <summary>
        /// Get info on a specific trigger order, see <see cref="GetFuturesTriggerOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesTriggerOrder>> GetFuturesTriggerOrderAsync(GetOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for getting a specific futures trigger order from the exchange via the REST API.
    /// </summary>
    public interface IGetFuturesTriggerOrderRest : IGetFuturesTriggerOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFuturesTriggerOrder>> GetFuturesTriggerOrderAsync(GetOrderRequest request, CancellationToken ct = default);
    }
}
