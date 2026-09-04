using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request definition for canceling a spot order by client order id on an exchange.
    /// </summary>
    public interface ICancelSpotOrderByClientOrderId: ISharedApiCapability
    {
        /// <summary>
        /// Spot cancel order by client order id request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelSpotOrderByClientOrderIdOptions CancelSpotOrderByClientOrderIdOptions { get; }
        /// <summary>
        /// Cancel a spot order using client order id, see <see cref="CancelSpotOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> CancelSpotOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Request definition for canceling a spot order by client order id on an exchange.
    /// </summary>
    public interface ICancelSpotOrderByClientOrderIdRest : ICancelSpotOrderByClientOrderId, ISharedRest
    {
        /// <summary>
        /// Cancel a spot order using client order id, see <see cref="CancelSpotOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<HttpResult<SharedId>> CancelSpotOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Request definition for canceling a spot order by client order id on an exchange.
    /// </summary>
    public interface ICancelSpotOrderByClientOrderIdSocket : ICancelSpotOrderByClientOrderId, ISharedSocket
    {
        /// <summary>
        /// Cancel a spot order using client order id, see <see cref="CancelSpotOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<QueryResult<SharedId>> CancelSpotOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct = default);
    }
}
