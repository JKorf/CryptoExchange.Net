using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
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
        /// <param name="baseAsset"></param>
        /// <param name="quoteAsset"></param>
        /// <returns></returns>
        string GetSymbolName(string baseAsset, string quoteAsset);

        /// <summary>
        /// Get a list of symbols for the exchange
        /// </summary>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Symbol>>> GetSymbolsAsync();

        /// <summary>
        /// Get a ticker for the exchange
        /// </summary>
        /// <param name="symbol">The symbol to get klines for</param>
        /// <returns></returns>
        Task<WebCallResult<Ticker>> GetTickerAsync(string symbol);

        /// <summary>
        /// Get a list of tickers for the exchange
        /// </summary>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Ticker>>> GetTickersAsync();

        /// <summary>
        /// Get a list of candles for a given symbol on the exchange
        /// </summary>
        /// <param name="symbol">The symbol to retrieve the candles for</param>
        /// <param name="timespan">The timespan to retrieve the candles for. The supported value are dependent on the exchange</param>
        /// <param name="startTime">[Optional] Start time to retrieve klines for</param>
        /// <param name="endTime">[Optional] End time to retrieve klines for</param>
        /// <param name="limit">[Optional] Max number of results</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Kline>>> GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null);

        /// <summary>
        /// Get the order book for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the book for</param>
        /// <returns></returns>
        Task<WebCallResult<CommonObjects.OrderBook>> GetOrderBookAsync(string symbol);

        /// <summary>
        /// The recent trades for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the trades for</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Trade>>> GetRecentTradesAsync(string symbol);

        /// <summary>
        /// Get balances
        /// </summary>
        /// <param name="accountId">[Optional] The account id to retrieve balances for, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Balance>>> GetBalancesAsync(string? accountId = null);

        /// <summary>
        /// Get an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<Order>> GetOrderAsync(string orderId, string? symbol = null);

        /// <summary>
        /// Get trades for an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<UserTrade>>> GetOrderTradesAsync(string orderId, string? symbol = null);

        /// <summary>
        /// Get a list of open orders
        /// </summary>
        /// <param name="symbol">[Optional] The symbol to get open orders for, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Order>>> GetOpenOrdersAsync(string? symbol = null);

        /// <summary>
        /// Get a list of closed orders
        /// </summary>
        /// <param name="symbol">[Optional] The symbol to get closed orders for, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Order>>> GetClosedOrdersAsync(string? symbol = null);

        /// <summary>
        /// Cancel an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<OrderId>> CancelOrderAsync(string orderId, string? symbol = null);
    }

    /// <summary>
    /// Common futures endpoints
    /// </summary>
    public interface IFuturesClient: IBaseRestClient
    {
        /// <summary>
        /// Place an order
        /// </summary>
        /// <param name="symbol">The symbol the order is for</param>
        /// <param name="side">The side of the order</param>
        /// <param name="type">The type of the order</param>
        /// <param name="quantity">The quantity of the order</param>
        /// <param name="price">The price of the order, only for limit orders</param>
        /// <param name="accountId">[Optional] The account id to place the order on, required for some exchanges, ignored otherwise</param>
        /// <param name="leverage">[Optional] Leverage for this order. This is needed for some exchanges. For exchanges where this is not needed this parameter is ignored (and should be set before hand)</param>
        /// <returns>The id of the resulting order</returns>
        Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, decimal? price = null, int? leverage = null, string? accountId = null);

        /// <summary>
        /// Get position
        /// </summary>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<Position>>> GetPositionsAsync();
    }

    /// <summary>
    /// Common spot endpoints
    /// </summary>
    public interface ISpotClient: IBaseRestClient
    {
        /// <summary>
        /// Place an order
        /// </summary>
        /// <param name="symbol">The symbol the order is for</param>
        /// <param name="side">The side of the order</param>
        /// <param name="type">The type of the order</param>
        /// <param name="quantity">The quantity of the order</param>
        /// <param name="price">The price of the order, only for limit orders</param>
        /// <param name="accountId">[Optional] The account id to place the order on, required for some exchanges, ignored otherwise</param>
        /// <returns>The id of the resulting order</returns>
        Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, decimal? price = null, string? accountId = null);
    }
}
