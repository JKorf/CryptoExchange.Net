using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request definition for retrieving an order book snapshot for a symbol on an exchange.
    /// </summary>
    public interface IGetOrderBook : ISharedApiCapability
    {
        /// <summary>
        /// Order book request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetOrderBookOptions GetOrderBookOptions { get; }

        /// <summary>
        /// Get the order book for a symbol, see <see cref="GetOrderBookOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedOrderBook>> GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Request definition for retrieving an order book snapshot for a symbol on an exchange.
    /// </summary>
    public interface IGetOrderBookRest : IGetOrderBook, ISharedRest
    {
        /// <summary>
        /// Get the order book for a symbol, see <see cref="GetOrderBookOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        new Task<HttpResult<SharedOrderBook>> GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct = default);
    }
}
