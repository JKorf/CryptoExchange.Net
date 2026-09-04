using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request definition for getting trades for a specific futures order on an exchange.
    /// </summary>
    public interface IGetFuturesOrderTrades : ISharedApiCapability
    {
        /// <summary>
        /// Futures get order trades request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesOrderTradesOptions GetFuturesOrderTradesOptions { get; }
        /// <summary>
        /// Get trades for a specific futures order, see <see cref="GetFuturesOrderTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedUserTrade[]>> GetFuturesOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Request definition for getting trades for a specific futures order on an exchange.
    /// </summary>
    public interface IGetFuturesOrderTradesRest : IGetFuturesOrderTrades, ISharedRest
    {
        /// <summary>
        /// Get trades for a specific futures order, see <see cref="GetFuturesOrderTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<HttpResult<SharedUserTrade[]>> GetFuturesOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);
    }
}
