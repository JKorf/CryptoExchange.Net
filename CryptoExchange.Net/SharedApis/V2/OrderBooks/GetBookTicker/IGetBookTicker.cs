using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving the best bid/ask prices for a symbol on an exchange.
    /// </summary>
    public interface IGetBookTicker : ISharedApiCapability
    {
        /// <summary>
        /// Book ticker request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetBookTickerOptions GetBookTickerOptions { get; }

        /// <summary>
        /// Get the best ask/bid info for a symbol, see <see cref="GetBookTickerOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedBookTicker>> GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving the best bid/ask prices for a symbol on an exchange via the REST API.
    /// </summary>
    public interface IGetBookTickerRest : IGetBookTicker, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedBookTicker>> GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct = default);
    }
}
