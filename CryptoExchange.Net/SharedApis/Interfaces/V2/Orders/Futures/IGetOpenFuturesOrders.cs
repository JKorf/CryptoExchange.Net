using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving open futures orders from an exchange.
    /// </summary>
    public interface IGetOpenFuturesOrders : ISharedApiCapability
    {
        /// <summary>
        /// Futures get open orders request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetOpenFuturesOrdersOptions GetOpenFuturesOrdersOptions { get; }
        /// <summary>
        /// Get info on a open futures orders, see <see cref="GetOpenFuturesOrdersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesOrder[]>> GetOpenFuturesOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving open futures orders from an exchange via the REST API.
    /// </summary>
    public interface IGetOpenFuturesOrdersRest : IGetOpenFuturesOrders, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFuturesOrder[]>> GetOpenFuturesOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct = default);
    }
}
