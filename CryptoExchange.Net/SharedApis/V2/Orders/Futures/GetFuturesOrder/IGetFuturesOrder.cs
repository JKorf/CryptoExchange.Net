using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving a futures order from an exchange.
    /// </summary>
    public interface IGetFuturesOrder : ISharedApiCapability
    {
        /// <summary>
        /// Futures get order request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesOrderOptions GetFuturesOrderOptions { get; }
        /// <summary>
        /// Get info on a specific futures order, see <see cref="GetFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesOrder>> GetFuturesOrderAsync(GetOrderRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving a futures order from an exchange via the REST API.
    /// </summary>
    public interface IGetFuturesOrderRest : IGetFuturesOrder, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFuturesOrder>> GetFuturesOrderAsync(GetOrderRequest request, CancellationToken ct = default);
    }
}
