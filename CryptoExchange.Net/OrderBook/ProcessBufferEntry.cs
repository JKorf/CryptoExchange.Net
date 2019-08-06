using System.Collections.Generic;

namespace CryptoExchange.Net.OrderBook
{
    /// <summary>
    /// Buffer entry for order book
    /// </summary>
    public class ProcessBufferEntry
    {
        /// <summary>
        /// The first sequence number of the entries
        /// </summary>
        public long FirstSequence { get; set; }
        /// <summary>
        /// The last sequence number of the entries
        /// </summary>
        public long LastSequence { get; set; }
        /// <summary>
        /// List of entries
        /// </summary>
        public List<ProcessEntry> Entries { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ProcessBufferEntry()
        {
            Entries = new List<ProcessEntry>();
        }
    }
}
