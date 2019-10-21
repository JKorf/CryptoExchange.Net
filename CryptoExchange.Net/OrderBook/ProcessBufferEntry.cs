using CryptoExchange.Net.Interfaces;
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
        /// List of asks
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Asks { get; set; } = new List<ISymbolOrderBookEntry>();
        /// <summary>
        /// List of bids
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Bids { get; set; } = new List<ISymbolOrderBookEntry>();
    }
}
