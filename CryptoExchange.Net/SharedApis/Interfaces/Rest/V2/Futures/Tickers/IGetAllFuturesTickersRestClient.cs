using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    public interface IGetAllFuturesTickersRestClient : ISharedClient
    {
        /// <summary>
        /// Futures get tickers request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesTickersOptions GetFuturesTickersOptions { get; }
        /// <summary>
        /// Get ticker info for all futures symbols, see <see cref="GetFuturesTickersOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedFuturesTicker[]>> GetFuturesTickersAsync(GetTickersRequest request, CancellationToken ct = default);
    }
}
