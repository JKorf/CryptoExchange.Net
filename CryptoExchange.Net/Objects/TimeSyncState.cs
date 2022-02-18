using System;
using System.Threading;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// The time synchronization state of an API client
    /// </summary>
    public class TimeSyncState
    {
        /// <summary>
        /// Semaphore to use for checking the time syncing. Should be shared instance among the API client
        /// </summary>
        public SemaphoreSlim Semaphore { get; }
        /// <summary>
        /// Last sync time for the API client
        /// </summary>
        public DateTime LastSyncTime { get; set; }
        /// <summary>
        /// Time offset for the API client
        /// </summary>
        public TimeSpan TimeOffset { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TimeSyncState()
        {
            Semaphore = new SemaphoreSlim(1, 1);
        }
    }

    /// <summary>
    /// Time synchronization info
    /// </summary>
    public class TimeSyncInfo
    {
        /// <summary>
        /// Logger
        /// </summary>
        public Log Log { get; }
        /// <summary>
        /// Should synchronize time
        /// </summary>
        public bool SyncTime { get; }
        /// <summary>
        /// Timestamp recalulcation interval
        /// </summary>
        public TimeSpan RecalculationInterval { get; }
        /// <summary>
        /// Time sync state for the API client
        /// </summary>
        public TimeSyncState TimeSyncState { get; }
       
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="syncTime"></param>
        /// <param name="syncState"></param>
        public TimeSyncInfo(Log log, bool syncTime, TimeSyncState syncState)
        {
            Log = log;
            SyncTime = syncTime;
            TimeSyncState = syncState;
        }

        /// <summary>
        /// Set the time offset
        /// </summary>
        /// <param name="offset"></param>
        public void UpdateTimeOffset(TimeSpan offset)
        {
            TimeSyncState.LastSyncTime = DateTime.UtcNow;
            if (offset.TotalMilliseconds > 0 && offset.TotalMilliseconds < 500)
            {
                Log.Write(LogLevel.Information, $"Time offset within limits, set offset to 0ms");
                TimeSyncState.TimeOffset = TimeSpan.Zero;
            }
            else
            {
                Log.Write(LogLevel.Information, $"Time offset set to {Math.Round(offset.TotalMilliseconds)}ms");
                TimeSyncState.TimeOffset = offset;
            }
        }
    }
}
