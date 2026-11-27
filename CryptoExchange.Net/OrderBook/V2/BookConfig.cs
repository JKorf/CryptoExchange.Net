using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.OrderBook.V2
{
    /// <summary>
    /// Order book configuration
    /// </summary>
    public class BookConfig
    {
        /// <summary>
        /// The amount of levels for this book
        /// </summary>
        public int? Levels { get; set; }

        /// <summary>
        /// Whether levels should be strictly enforced. For example, when an order book has 25 levels and a new update comes in which pushes
        /// the current level 25 ask out of the top 25, should the level 26 entry be removed from the book or does the server handle this
        /// </summary>
        public bool StrictLevels { get; set; } = false;

        /// <summary>
        /// Whether update numbers are consecutive. If set to true and an update comes in which isn't the previous sequences number + 1
        /// the book will resynchronize as it is deemed out of sync
        /// </summary>
        public bool SequencesAreConsecutive { get; set; } = false;

        /// <summary>
        /// Whether the first update message after a snapshot may have overlapping sequence numbers instead of the snapshot sequence number + 1
        /// </summary>
        public bool SkipSequenceCheckFirstUpdateAfterSnapshotSet { get; set; } = false;
        /// <summary>
        /// Min wait time until an update is buffered
        /// </summary>
        public TimeSpan MinFirstBufferedUpdateWait { get; set; } = TimeSpan.FromMilliseconds(250);
        /// <summary>
        /// Max wait time until an update is buffered
        /// </summary>
        public TimeSpan MaxFirstBufferedUpdateWait { get; set; } = TimeSpan.FromMilliseconds(500);
        /// <summary>
        /// Max wait time for an order book to reach synced status
        /// </summary>
        public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
