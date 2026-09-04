using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request definition for getting a specific spot trigger order on an exchange
    /// </summary>
    public interface IGetSpotTriggerOrder : ISharedApiCapability
    {
        /// <summary>
        /// Get trigger order request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
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
    /// Request definition for getting a specific spot trigger order on an exchange
    /// </summary>
    public interface IGetSpotTriggerOrderRest : IGetSpotTriggerOrder, ISharedRest
    {
        /// <summary>
        /// Get info on a specific trigger order, see <see cref="GetSpotTriggerOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<HttpResult<SharedSpotTriggerOrder>> GetSpotTriggerOrderAsync(GetOrderRequest request, CancellationToken ct = default);
    }
}
