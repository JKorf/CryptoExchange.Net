using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces.CommonClients
{
    /// <summary>
    /// Common rest client endpoints
    /// </summary>
    public interface IBaseRestClient
    {
        /// <summary>
        /// The name of the exchange
        /// </summary>
        string ExchangeName { get; }

        /// <summary>
        /// Should be triggered on order placing
        /// </summary>
        event Action<OrderId> OnOrderPlaced;
        /// <summary>
        /// Should be triggered on order cancelling
        /// </summary>
        event Action<OrderId> OnOrderCanceled;

        /// <summary>
        /// Get the symbol name based on a base and quote asset
        /// </summary>
        /// <param name="baseAsset">The base asset</param>
        /// <param name="quoteAsset">The quote asset</param>
        /// <returns></returns>
        string GetSymbolName(string baseAsset, string quoteAsset);

        /// <summary>
        /// Get a list of symbols for the exchange
        /// </summary>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Symbol>>> GetSymbolsAsync(CancellationToken ct = default);

        /// <summary>
        /// Get a ticker for the exchange
        /// </summary>
        /// <param name="symbol">The symbol to get klines for</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<Ticker>> GetTickerAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get a list of tickers for the exchange
        /// </summary>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Ticker>>> GetTickersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get a list of candles for a given symbol on the exchange
        /// </summary>
        /// <param name="symbol">The symbol to retrieve the candles for</param>
        /// <param name="timespan">The timespan to retrieve the candles for. The supported value are dependent on the exchange</param>
        /// <param name="startTime">[Optional] Start time to retrieve klines for</param>
        /// <param name="endTime">[Optional] End time to retrieve klines for</param>
        /// <param name="limit">[Optional] Max number of results</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Kline>>> GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, CancellationToken ct = default);

        /// <summary>
        /// Get the order book for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the book for</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<CommonObjects.OrderBook>> GetOrderBookAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// The recent trades for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the trades for</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Trade>>> GetRecentTradesAsync(string symbol, CancellationToken ct = default);

        /// <summary>
        /// Get balances
        /// </summary>
        /// <param name="accountId">[Optional] The account id to retrieve balances for, required for some exchanges, ignored otherwise</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Balance>>> GetBalancesAsync(string? accountId = null, CancellationToken ct = default);

        /// <summary>
        /// Get an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<Order>> GetOrderAsync(string orderId, string? symbol = null, CancellationToken ct = default);

        /// <summary>
        /// Get trades for an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<UserTrade>>> GetOrderTradesAsync(string orderId, string? symbol = null, CancellationToken ct = default);

        /// <summary>
        /// Get a list of open orders
        /// </summary>
        /// <param name="symbol">[Optional] The symbol to get open orders for, required for some exchanges, ignored otherwise</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Order>>> GetOpenOrdersAsync(string? symbol = null, CancellationToken ct = default);

        /// <summary>
        /// Get a list of closed orders
        /// </summary>
        /// <param name="symbol">[Optional] The symbol to get closed orders for, required for some exchanges, ignored otherwise</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Order>>> GetClosedOrdersAsync(string? symbol = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <param name="ct">[Optional] Cancellation token for cancelling the request</param>
        /// <returns></returns>
        Task<WebCallResult<OrderId>> CancelOrderAsync(string orderId, string? symbol = null, CancellationToken ct = default);
    }
}
