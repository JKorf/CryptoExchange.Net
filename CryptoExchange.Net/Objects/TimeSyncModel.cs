using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CryptoExchange.Net.Objects
{
    public class TimeSyncModel
    {
        public bool SyncTime { get; set; }
        public SemaphoreSlim Semaphore { get; set; }
        public DateTime LastSyncTime { get; set; }

        public TimeSyncModel(bool syncTime, SemaphoreSlim semaphore, DateTime lastSyncTime)
        {
            SyncTime = syncTime;
            Semaphore = semaphore;
            LastSyncTime = lastSyncTime;
        }
    }
}
