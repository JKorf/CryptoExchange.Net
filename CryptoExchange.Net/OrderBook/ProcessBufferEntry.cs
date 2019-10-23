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
        /// List of asks
        /// </summary>
        public IEnumerable<ISymbolOrderSequencedBookEntry> Asks { get; set; } = new List<ISymbolOrderSequencedBookEntry>();
        /// <summary>
        /// List of bids
        /// </summary>
        public IEnumerable<ISymbolOrderSequencedBookEntry> Bids { get; set; } = new List<ISymbolOrderSequencedBookEntry>();
    }

    /// <summary>
    /// Buffer entry with a single update id per update
    /// </summary>
    public class ProcessBufferSingleSequenceEntry
    {
        /// <summary>
        /// First update id
        /// </summary>
        public long UpdateId { get; set; }
        /// <summary>
        /// List of asks
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Asks { get; set; } = new List<ISymbolOrderBookEntry>();
        /// <summary>
        /// List of bids
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Bids { get; set; } = new List<ISymbolOrderBookEntry>();
    }

    /// <summary>
    /// Buffer entry with a first and last update id
    /// </summary>
    public class ProcessBufferRangeSequenceEntry
    {
        /// <summary>
        /// First update id
        /// </summary>
        public long FirstUpdateId { get; set; }
        /// <summary>
        /// Last update id
        /// </summary>
        public long LastUpdateId { get; set; }
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
