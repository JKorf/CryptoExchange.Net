using CryptoExchange.Net.Interfaces;
using System.Collections.Generic;

namespace CryptoExchange.Net.OrderBook
{
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
