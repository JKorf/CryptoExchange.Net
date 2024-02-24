using CryptoExchange.Net.Objects;
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
        /// Sync status
        /// </summary>
        SyncStatus Status { get; }
        /// <summary>
        /// Event for when a new kline is added
        /// </summary>
        event Func<IKlineItem, Task>? Added;
        /// <summary>
        /// Event for when a kline is removed because it's no longer within the period/limit window
        /// </summary>
        event Func<IKlineItem, Task>? Removed;
        /// <summary>
        /// Event for when the initial snapshot is set
        /// </summary>
        event Func<IEnumerable<IKlineItem>, Task>? SnapshotSet;
        /// <summary>
        /// Event for when a kline is updated
        /// </summary>
        event Func<IKlineItem, Task> Updated;
        /// <summary>
        /// Event for when the sync status changes
        /// </summary>
        event Func<SyncStatus, SyncStatus, Task>? OnStatusChanged;

        /// <summary>
        /// The average volume per kline, excluding the last (uncompleted) one
        /// </summary>
        /// <returns></returns>
        decimal AverageVolume();

        /// <summary>
        /// Get the data tracked
        /// </summary>
        /// <param name="since">Return data after his timestamp</param>
        /// <returns></returns>
        IEnumerable<IKlineItem> GetData(DateTime? since = null);

        /// <summary>
        /// The highest price across all klines
        /// </summary>
        /// <returns></returns>
        decimal HighPrice();

        /// <summary>
        /// The lowest price across all klines
        /// </summary>
        /// <returns></returns>
        decimal LowPrice();

        /// <summary>
        /// Start synchronization
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Stop synchronization
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// The total volume
        /// </summary>
        /// <returns></returns>
        decimal Volume();
    }
}