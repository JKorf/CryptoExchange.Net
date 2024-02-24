using CryptoExchange.Net.Objects;
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
        /// From which timestamp the trades are registered
        /// </summary>
        DateTime SyncedFrom { get; }

        /// <summary>
        /// The current synchronization status
        /// </summary>
        SyncStatus Status { get; }

        /// <summary>
        /// Event for when a new trade is added
        /// </summary>
        event Func<ITradeItem, Task>? OnAdded;
        /// <summary>
        /// Event for when a trade is removed because it's no longer within the period/limit window
        /// </summary>
        event Func<ITradeItem, Task>? OnRemoved;
        /// <summary>
        /// Event for when the initial snapshot is set
        /// </summary>
        event Func<IEnumerable<ITradeItem>, Task>? OnSnapshotSet;
        /// <summary>
        /// Event for when the sync status changes
        /// </summary>
        event Func<SyncStatus, SyncStatus, Task>? OnStatusChanged;

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
        /// Get the data tracked
        /// </summary>
        /// <param name="since">Return data after his timestamp</param>
        /// <returns></returns>
        IEnumerable<ITradeItem> GetData(DateTime? since = null);

        /// <summary>
        /// The average price across all trades
        /// </summary>
        /// <returns></returns>
        decimal AveragePrice();

        /// <summary>
        /// The average price in the last period
        /// </summary>
        /// <param name="period">Period to get the average price for</param>
        /// <returns></returns>
        decimal AveragePriceForLast(TimeSpan period);

        /// <summary>
        /// The average price since a specific time timestamp
        /// </summary>
        /// <param name="timestamp">Timestamp to get the average price since</param>
        /// <returns></returns>
        decimal AveragePriceSince(DateTime timestamp);

        /// <summary>
        /// The qoute volume across all trades
        /// </summary>
        /// <returns></returns>
        decimal QuoteVolume();

        /// <summary>
        /// The quote volume in the last time period
        /// </summary>
        /// <param name="period">Period to get the quote volume for</param>
        /// <returns></returns>
        decimal QuoteVolumeForLast(TimeSpan period);

        /// <summary>
        /// The quote volume since a specific time timestamp
        /// </summary>
        /// <param name="timestamp">Timestamp to get the quote volume since</param>
        /// <returns></returns>
        decimal QuoteVolumeSince(DateTime timestamp);

        /// <summary>
        /// The trade volume across all trades
        /// </summary>
        /// <returns></returns>
        decimal Volume();

        /// <summary>
        /// The trade volume in the last time period
        /// </summary>
        /// <param name="period">Period to get the trade volume for</param>
        /// <returns></returns>
        decimal VolumeForLast(TimeSpan period);

        /// <summary>
        /// The trade volume since a specific time timestamp
        /// </summary>
        /// <param name="timestamp">Timestamp to get the trade volume since</param>
        /// <returns></returns>
        decimal VolumeSince(DateTime timestamp);
    }
}