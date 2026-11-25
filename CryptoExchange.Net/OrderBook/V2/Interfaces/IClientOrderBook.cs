using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.OrderBook.V2.Interfaces
{
    /// <summary>
    /// Client order book
    /// </summary>
    public interface IClientOrderBook
    {
        /// <summary>
        /// The exchange the book is for
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// The status of the order book
        /// </summary>
        BookStatus Status { get; set; }
        /// <summary>
        /// The synchronization status of the order book
        /// </summary>
        SyncStatus SyncStatus { get; set; }

        /// <summary>
        /// Last update identifier
        /// </summary>
        long LastSequenceNumber { get; }
        /// <summary>
        /// The symbol of the order book
        /// </summary>
        SharedSymbol Symbol { get; }

        /// <summary>
        /// Event when the status changes
        /// </summary>
        event Action<BookStatus> OnStatusChange;
        /// <summary>
        /// Event when the synchronization status changes
        /// </summary>
        event Action<SyncStatus> OnSyncStatusChange;
        /// <summary>
        /// Timestamp of when the last update was applied to the book, local time
        /// </summary>
        DateTime UpdateTime { get; }
        /// <summary>
        /// Timestamp of the last event that was applied, server time
        /// </summary>
        DateTime? UpdateServerTime { get; }
        /// <summary>
        /// Timestamp of the last event that was applied, in local time, estimated based on timestamp difference between client and server
        /// </summary>
        DateTime? UpdateLocalTime { get; }
        /// <summary>
        /// Age of the data, in local time, estimated based on timestamp difference between client and server + the period since last update
        /// </summary>
        TimeSpan? DataAge { get; }

        /// <summary>
        /// Get a snapshot of the book at this moment
        /// </summary>
        BookSnapshot Book { get; }

        /// <summary>
        /// The list of asks
        /// </summary>
        IReadOnlyDictionary<decimal, decimal> Asks { get; }

        /// <summary>
        /// The list of bids
        /// </summary>
        IReadOnlyDictionary<decimal, decimal> Bids { get; }

        /// <summary>
        /// The best bid currently in the order book
        /// </summary>
        (decimal, decimal) BestBid { get; }

        /// <summary>
        /// The best ask currently in the order book
        /// </summary>
        (decimal, decimal) BestAsk { get; }

        /// <summary>
        /// Get the top x levels of the order book
        /// </summary>
        BookSnapshot GetLevels(int levels);

        /// <summary>
        /// Start connecting and synchronizing the order book
        /// </summary>
        /// <param name="ct">A cancellation token to stop the order book when canceled</param>
        /// <returns></returns>
        Task<CallResult> StartAsync(CancellationToken? ct = null);

        /// <summary>
        /// Register a handler to be called when the order book changes
        /// </summary>
        /// <param name="handler">The handler to call</param>
        void RegisterChangedHandler(Action<BookSnapshot> handler);

        /// <summary>
        /// Register a handler to be called when the top entries of the order book change
        /// </summary>
        /// <param name="top">The number of top entries to watch for changes</param>
        /// <param name="handler">The handler to call</param>
        void RegisterChangedHandler(int top, Action<BookSnapshot> handler);

        /// <summary>
        /// Unregister a specific change handler
        /// </summary>
        /// <param name="handler">The handler to unregister</param>
        void UnregisterChangedHandler(Action<BookSnapshot> handler);

        /// <summary>
        /// Unregister all change handlers
        /// </summary>
        void UnregisterChangedHandlers();

        /// <summary>
        /// Stop syncing the order book
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Get the average price that a market order would fill at at the current order book state. This is no guarantee that an order of that quantity would actually be filled
        /// at that price since between this calculation and the order placement the book might have changed.
        /// </summary>
        /// <param name="quantity">The quantity in base asset to fill</param>
        /// <param name="type">The type</param>
        /// <returns>Average fill price</returns>
        CallResult<decimal> CalculateAverageFillPrice(decimal quantity, OrderBookEntryType type);

        /// <summary>
        /// Get the amount of base asset which can be traded with the quote quantity when placing a market order at at the current order book state. 
        /// This is no guarantee that an order of that quantity would actually be fill the quantity returned by this since between this calculation and the order placement the book might have changed.
        /// </summary>
        /// <param name="quoteQuantity">The quantity in quote asset looking to trade</param>
        /// <param name="type">The type</param>
        /// <returns>Amount of base asset tradable with the specified amount of quote asset</returns>
        CallResult<decimal> CalculateTradableAmount(decimal quoteQuantity, OrderBookEntryType type);

        /// <summary>
        /// String representation of the top x entries
        /// </summary>
        /// <returns></returns>
        string ToString(int rows);

        /// <summary>
        /// Output the orderbook to the console
        /// </summary>
        /// <param name="numberOfEntries">Number of rows to display</param>
        /// <param name="refreshInterval">Refresh interval</param>
        /// <param name="ct">Cancellation token</param>
        Task OutputToConsoleAsync(int numberOfEntries, TimeSpan refreshInterval, CancellationToken ct = default);
    }
}
