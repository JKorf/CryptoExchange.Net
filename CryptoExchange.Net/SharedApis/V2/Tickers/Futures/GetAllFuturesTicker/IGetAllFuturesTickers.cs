using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving all futures tickers from an exchange.
    /// </summary>
    public interface IGetAllFuturesTickers : ISharedApiCapability
    {
        /// <summary>
        /// Futures get tickers request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetAllFuturesTickersOptions GetAllFuturesTickersOptions { get; }
        /// <summary>
        /// Get ticker info for all futures symbols, see <see cref="GetAllFuturesTickersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesTicker[]>> GetAllFuturesTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving all futures tickers from an exchange via the REST API.
    /// </summary>
    public interface IGetAllFuturesTickersRest : IGetAllFuturesTickers
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFuturesTicker[]>> GetAllFuturesTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }
}
