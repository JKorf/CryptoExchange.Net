namespace CryptoExchange.Net.OrderBook.V2
{
    /// <summary>
    /// Synchronization status
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// Idle, not running
        /// </summary>
        Idle,
        /// <summary>
        /// Currently syncing
        /// </summary>
        Syncing,
        /// <summary>
        /// Is synced
        /// </summary>
        Synced,
        /// <summary>
        /// Out of sync, will try to recover
        /// </summary>
        OutOfSync
    }
}
