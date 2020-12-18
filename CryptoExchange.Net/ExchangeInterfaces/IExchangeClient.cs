﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Shared interface for exchange wrappers based on the CryptoExchange.Net package
    /// </summary>
    public interface IExchangeClient
    {
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
        Task<WebCallResult<IEnumerable<ICommonSymbol>>> GetSymbolsAsync();

        /// <summary>
        /// Get a list of tickers for the exchange
        /// </summary>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<ICommonTicker>>> GetTickersAsync();

        /// <summary>
        /// Get a list of candles for a given symbol on the exchange
        /// </summary>
        /// <param name="symbol">The symbol to retrieve the candles for</param>
        /// <param name="timespan">The timespan to retrieve the candles for. The supported value are dependent on the exchange</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<ICommonKline>>> GetKlinesAsync(string symbol, TimeSpan timespan);
        /// <summary>
        /// Get the order book for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the book for</param>
        /// <returns></returns>
        Task<WebCallResult<ICommonOrderBook>> GetOrderBookAsync(string symbol);
        /// <summary>
        /// The recent trades for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the trades for</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<ICommonRecentTrade>>> GetRecentTradesAsync(string symbol);

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
        Task<WebCallResult<ICommonOrderId>> PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, decimal? price = null, string? accountId = null);
        /// <summary>
        /// Get an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<ICommonOrder>> GetOrderAsync(string orderId, string? symbol = null);
        /// <summary>
        /// Get trades for an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<ICommonTrade>>> GetTradesAsync(string orderId, string? symbol = null);
        /// <summary>
        /// Get a list of open orders
        /// </summary>
        /// <param name="symbol">[Optional] The symbol to get open orders for, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<ICommonOrder>>> GetOpenOrdersAsync(string? symbol);

        /// <summary>
        /// Get a list of closed orders
        /// </summary>
        /// <param name="symbol">[Optional] The symbol to get closed orders for, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<IEnumerable<ICommonOrder>>> GetClosedOrdersAsync(string? symbol);
        /// <summary>
        /// Cancel an order by id
        /// </summary>
        /// <param name="orderId">The id</param>
        /// <param name="symbol">[Optional] The symbol the order is on, required for some exchanges, ignored otherwise</param>
        /// <returns></returns>
        Task<WebCallResult<ICommonOrderId>> CancelOrderAsync(string orderId, string? symbol);

        /// <summary>
        /// Common order id
        /// </summary>
        public enum OrderType
        {
            /// <summary>
            /// Limit type
            /// </summary>
            Limit,
            /// <summary>
            /// Market type
            /// </summary>
            Market,
            /// <summary>
            /// Other order type
            /// </summary>
            Other
        }

        /// <summary>
        /// Common order side
        /// </summary>
        public enum OrderSide
        {
            /// <summary>
            /// Buy order
            /// </summary>
            Buy,
            /// <summary>
            /// Sell order
            /// </summary>
            Sell
        }
    }
}
