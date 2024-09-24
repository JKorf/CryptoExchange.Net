using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving the most recent public trades
    /// </summary>
    public interface IRecentTradeRestClient : ISharedClient
    {
        /// <summary>
        /// Recent trades request options
        /// </summary>
        GetRecentTradesOptions GetRecentTradesOptions { get; }

        /// <summary>
        /// Get the most recent public trades
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<IEnumerable<SharedTrade>>> GetRecentTradesAsync(GetRecentTradesRequest request, CancellationToken ct = default);
    }
}
