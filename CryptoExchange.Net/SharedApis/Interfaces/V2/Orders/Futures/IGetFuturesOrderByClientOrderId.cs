using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Endpoint definition for retrieving a futures order by client order id on an exchange.
    /// </summary>
    public interface IGetFuturesOrderByClientOrderId : ISharedApiCapability
    {
        /// <summary>
        /// Futures get order by client order id request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesOrderByClientOrderIdOptions GetFuturesOrderByClientOrderIdOptions { get; }

        /// <summary>
        /// Get info on a specific futures order using a client order id, see <see cref="GetFuturesOrderByClientOrderIdOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesOrder>> GetFuturesOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Endpoint definition for retrieving a futures order by client order id on an exchange.
    /// </summary>
    public interface IGetFuturesOrderByClientOrderIdRest : IGetFuturesOrderByClientOrderId, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFuturesOrder>> GetFuturesOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct = default);
    }
}
