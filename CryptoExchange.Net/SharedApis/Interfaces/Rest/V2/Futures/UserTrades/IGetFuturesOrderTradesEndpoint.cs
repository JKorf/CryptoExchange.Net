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
    public interface IGetFuturesOrderTradesEndpoint : ISharedApiEndpoint
    {
        /// <summary>
        /// Futures get order trades request options.<br />
        /// Use <see cref="EndpointOptions{TRequest, TClient}.RequiredRequestParameters"/>, <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesOrderTradesOptions GetFuturesOrderTradesOptions { get; }
        /// <summary>
        /// Get trades for a specific futures order, see <see cref="GetFuturesOrderTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedUserTrade[]>> GetFuturesOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);
    }
}
