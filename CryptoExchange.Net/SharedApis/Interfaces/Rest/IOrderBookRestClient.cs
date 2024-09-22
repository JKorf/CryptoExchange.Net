using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Options.Endpoints;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest
{
    /// <summary>
    /// Client for retrieving the order book for a symbol
    /// </summary>
    public interface IOrderBookRestClient : ISharedClient
    {
        /// <summary>
        /// Order book request options
        /// </summary>
        GetOrderBookOptions GetOrderBookOptions { get; }

        /// <summary>
        /// Get the order book for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<SharedOrderBook>> GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct = default);
    }
}
