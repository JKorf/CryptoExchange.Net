using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request definition for placing a futures trigger order on an exchange.
    /// </summary>
    public interface IPlaceFuturesTriggerOrder : ISharedApiCapability
    {
        /// <summary>
        /// Place futures trigger order options.<br />
        /// Use  <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceFuturesTriggerOrderOptions PlaceFuturesTriggerOrderOptions { get; }

        /// <summary>
        /// Place a new trigger order, see <see cref="PlaceFuturesTriggerOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedId>> PlaceFuturesTriggerOrderAsync(PlaceFuturesTriggerOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Request definition for placing a futures trigger order on an exchange.
    /// </summary>
    public interface IPlaceFuturesTriggerOrderRest : IPlaceFuturesTriggerOrder, ISharedRest
    {
        /// <summary>
        /// Place a new trigger order, see <see cref="PlaceFuturesTriggerOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        new Task<HttpResult<SharedId>> PlaceFuturesTriggerOrderAsync(PlaceFuturesTriggerOrderRequest request, CancellationToken ct = default);
    }
}
