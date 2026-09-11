using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// The type of updates the incremental order book subscription produces
    /// </summary>
    public enum SharedOrderBookSubscriptionType
    {
        /// <summary>
        /// Subscription produces an initial snapshot update, followed by incremental change updates
        /// </summary>
        SnapshotThenIncremental,
        /// <summary>
        /// Subscription produces only incremental change updates
        /// </summary>
        OnlyIncremental
    }
}
