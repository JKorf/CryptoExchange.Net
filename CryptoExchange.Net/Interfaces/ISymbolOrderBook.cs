using System;
using System.Collections.Generic;
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
        event Action<IEnumerable<ISymbolOrderBookEntry>, IEnumerable<ISymbolOrderBookEntry>> OnOrderBookUpdate;
        /// <summary>
        /// Event when the BestBid or BestAsk changes ie a Pricing Tick
        /// </summary>
        event Action<ISymbolOrderBookEntry, ISymbolOrderBookEntry> OnBestOffersChanged;
        /// <summary>
        /// Timestamp of the last update
        /// </summary>
        DateTime LastOrderBookUpdate { get; }

        /// <summary>
        /// The number of asks in the book
        /// </summary>
        int AskCount { get; }
        /// <summary>
        /// The number of bids in the book
        /// </summary>
        int BidCount { get; }

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
        /// <returns></returns>
        CallResult<bool> Start();

        /// <summary>
        /// Start connecting and synchronizing the order book
        /// </summary>
        /// <returns></returns>
        Task<CallResult<bool>> StartAsync();

        /// <summary>
        /// Stop syncing the order book
        /// </summary>
        /// <returns></returns>
        void Stop();

        /// <summary>
        /// Stop syncing the order book
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
