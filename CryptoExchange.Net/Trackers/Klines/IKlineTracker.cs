using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Klines
{
    /// <summary>
    /// A tracker for kline data of a symbol
    /// </summary>
    public interface IKlineTracker
    {
        /// <summary>
        /// The total number of klines
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
        /// The max number of klines tracked
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
        /// Sync status
        /// </summary>
        SyncStatus Status { get; }

        /// <summary>
        /// Get the last kline
        /// </summary>
        SharedKline? Last { get; }

        /// <summary>
        /// Event for when a new kline is added
        /// </summary>
        event Func<SharedKline, Task>? OnAdded;
        /// <summary>
        /// Event for when a kline is removed because it's no longer within the period/limit window
        /// </summary>
        event Func<SharedKline, Task>? OnRemoved;
        /// <summary>
        /// Event for when a kline is updated
        /// </summary>
        event Func<SharedKline, Task> OnUpdated;
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
        SharedKline[] GetData(DateTime? fromTimestamp = null, DateTime? toTimestamp = null);

        /// <summary>
        /// Get statistics on the klines
        /// </summary>
        /// <param name="fromTimestamp">Start timestamp to get the data from, defaults to tracked data start time</param>
        /// <param name="toTimestamp">End timestamp to get the data until, defaults to current time</param>
        /// <returns></returns>
        KlinesStats GetStats(DateTime? fromTimestamp = null, DateTime? toTimestamp = null);

    }
}