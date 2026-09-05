using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving the most recent trades for a symbol on an exchange.
    /// </summary>
    public interface IGetRecentTrades : ISharedApiCapability
    {
        /// <summary>
        /// Recent trades request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetRecentTradesOptions GetRecentTradesOptions { get; }

        /// <summary>
        /// Get the most recent public trades, see <see cref="GetRecentTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedTrade[]>> GetRecentTradesAsync(GetRecentTradesRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving the most recent trades for a symbol on an exchange via the REST API.
    /// </summary>
    public interface IGetRecentTradesRest : IGetRecentTrades, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedTrade[]>> GetRecentTradesAsync(GetRecentTradesRequest request, CancellationToken ct = default);
    }
}
