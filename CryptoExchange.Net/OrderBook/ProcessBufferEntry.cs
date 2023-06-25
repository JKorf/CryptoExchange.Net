using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.OrderBook
{
    /// <summary>
    /// Buffer entry with a first and last update id
    /// </summary>
    public class ProcessBufferRangeSequenceEntry
    {
        /// <summary>
        /// First sequence number in this update
        /// </summary>
        public long FirstUpdateId { get; set; }

        /// <summary>
        /// Last sequence number in this update
        /// </summary>
        public long LastUpdateId { get; set; }

        /// <summary>
        /// List of changed/new asks
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Asks { get; set; } = Array.Empty<ISymbolOrderBookEntry>();

        /// <summary>
        /// List of changed/new bids
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Bids { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
    }
}
