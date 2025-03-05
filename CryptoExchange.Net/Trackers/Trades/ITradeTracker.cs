using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Trades
{
    /// <summary>
    /// A tracker for trades on a symbol
    /// </summary>
    public interface ITradeTracker
    {
        /// <summary>
        /// The total number of trades
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Exchange name
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// Symbol name
        /// </summary>
        string SymbolName { get; }

        /// <summary>
        /// Symbol
        /// </summary>
        SharedSymbol Symbol { get; }

        /// <summary>
        /// The max number of trades tracked
        /// </summary>
        int? Limit { get; }

        /// <summary>
        /// The max age of the data tracked
        /// </summary>
        TimeSpan? Period { get; }

        /// <summary>
        /// From which timestamp the trades are registered
        /// </summary>
        DateTime? SyncedFrom { get; }

        /// <summary>
        /// The current synchronization status
        /// </summary>
        SyncStatus Status { get; }

        /// <summary>
        /// Get the last trade
        /// </summary>
        SharedTrade? Last { get; }

        /// <summary>
        /// Event for when a new trade is added
        /// </summary>
        event Func<SharedTrade, Task>? OnAdded;
        /// <summary>
        /// Event for when a trade is removed because it's no longer within the period/limit window
        /// </summary>
        event Func<SharedTrade, Task>? OnRemoved;
        /// <summary>
        /// Event for when the sync status changes
        /// </summary>
        event Func<SyncStatus, SyncStatus, Task>? OnStatusChanged;

        /// <summary>
        /// Start synchronization
        /// </summary>
        /// <returns></returns>
        Task<CallResult> StartAsync(bool startWithSnapshot = true);

        /// <summary>
        /// Stop synchronization
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Get the data tracked
        /// </summary>
        /// <param name="fromTimestamp">Start timestamp to get the data from, defaults to tracked data start time</param>
        /// <param name="toTimestamp">End timestamp to get the data until, defaults to current time</param>
        /// <returns></returns>
        SharedTrade[] GetData(DateTime? fromTimestamp = null, DateTime? toTimestamp = null);

        /// <summary>
        /// Get statistics on the trades
        /// </summary>
        /// <param name="fromTimestamp">Start timestamp to get the data from, defaults to tracked data start time</param>
        /// <param name="toTimestamp">End timestamp to get the data until, defaults to current time</param>
        /// <returns></returns>
        TradesStats GetStats(DateTime? fromTimestamp = null, DateTime? toTimestamp = null);
    }
}