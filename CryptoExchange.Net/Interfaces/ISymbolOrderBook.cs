using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Interface for order book
    /// </summary>
    public interface ISymbolOrderBook
    {
        /// <summary>
        /// The exchange the book is for
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// The Api the book is for
        /// </summary>
        string Api { get; }

        /// <summary>
        /// The status of the order book. Order book is up to date when the status is `Synced`
        /// </summary>
        OrderBookStatus Status { get; set; }

        /// <summary>
        /// Last update identifier
        /// </summary>
        long LastSequenceNumber { get; }
        /// <summary>
        /// The symbol of the order book
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// Event when the state changes
        /// </summary>
        event Action<OrderBookStatus, OrderBookStatus> OnStatusChange;
        /// <summary>
        /// Event when order book was updated. Be careful! It can generate a lot of events at high-liquidity markets
        /// </summary>    
        event Action<(IEnumerable<ISymbolOrderBookEntry> Bids, IEnumerable<ISymbolOrderBookEntry> Asks)> OnOrderBookUpdate;
        /// <summary>
        /// Event when the BestBid or BestAsk changes ie a Pricing Tick
        /// </summary>
        event Action<(ISymbolOrderBookEntry BestBid, ISymbolOrderBookEntry BestAsk)> OnBestOffersChanged;
        /// <summary>
        /// Timestamp of the last update
        /// </summary>
        DateTime UpdateTime { get; }

        /// <summary>
        /// The number of asks in the book
        /// </summary>
        int AskCount { get; }
        /// <summary>
        /// The number of bids in the book
        /// </summary>
        int BidCount { get; }

        /// <summary>
        /// Get a snapshot of the book at this moment
        /// </summary>
        (IEnumerable<ISymbolOrderBookEntry> bids, IEnumerable<ISymbolOrderBookEntry> asks) Book { get; }

        /// <summary>
        /// The list of asks
        /// </summary>
        IEnumerable<ISymbolOrderBookEntry> Asks { get; }

        /// <summary>
        /// The list of bids
        /// </summary>
        IEnumerable<ISymbolOrderBookEntry> Bids { get; }

        /// <summary>
        /// The best bid currently in the order book
        /// </summary>
        ISymbolOrderBookEntry BestBid { get; }

        /// <summary>
        /// The best ask currently in the order book
        /// </summary>
        ISymbolOrderBookEntry BestAsk { get; }

        /// <summary>
        /// BestBid/BesAsk returned as a pair
        /// </summary>
        (ISymbolOrderBookEntry Bid, ISymbolOrderBookEntry Ask) BestOffers { get; }

        /// <summary>
        /// Start connecting and synchronizing the order book
        /// </summary>
        /// <param name="ct">A cancellation token to stop the order book when canceled</param>
        /// <returns></returns>
        Task<CallResult<bool>> StartAsync(CancellationToken? ct = null);

        /// <summary>
        /// Stop syncing the order book
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Get the average price that a market order would fill at at the current order book state. This is no guarentee that an order of that quantity would actually be filled
        /// at that price since between this calculation and the order placement the book might have changed.
        /// </summary>
        /// <param name="quantity">The quantity in base asset to fill</param>
        /// <param name="type">The type</param>
        /// <returns>Average fill price</returns>
        CallResult<decimal> CalculateAverageFillPrice(decimal quantity, OrderBookEntryType type);

        /// <summary>
        /// Get the amount of base asset which can be traded with the quote quantity when placing a market order at at the current order book state. 
        /// This is no guarentee that an order of that quantity would actually be fill the quantity returned by this since between this calculation and the order placement the book might have changed.
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
    }
}
