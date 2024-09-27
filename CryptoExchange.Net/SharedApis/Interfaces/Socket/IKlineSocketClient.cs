using System;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects.Sockets;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for subscribing to kline/candlestick updates for a symbol
    /// </summary>
    public interface IKlineSocketClient : ISharedClient
    {
        /// <summary>
        /// Kline subscription options
        /// </summary>
        SubscribeKlineOptions SubscribeKlineOptions { get; }

        /// <summary>
        /// Subscribe to kline/candlestick updates for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<ExchangeResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(SubscribeKlineRequest request, Action<ExchangeEvent<SharedKline>> handler, CancellationToken ct = default);
    }
}
