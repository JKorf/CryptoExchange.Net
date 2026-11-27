using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.OrderBook.V2
{
    /// <summary>
    /// Order book update
    /// </summary>
    public class OrderBookUpdate
    {
        /// <summary>
        /// Local data time
        /// </summary>
        public DateTime? LocalDataTime { get; set; }
        /// <summary>
        /// Server data time
        /// </summary>
        public DateTime? ServerDataTime { get; set; }
        /// <summary>
        /// Start sequence number
        /// </summary>
        public long StartSequenceNumber { get; set; }
        /// <summary>
        /// End sequence number
        /// </summary>
        public long EndSequenceNumber { get; set; }
        /// <summary>
        /// Bids
        /// </summary>
        public IDictionary<decimal, decimal> Bids { get; set; } = new SortedList<decimal, decimal>();
        /// <summary>
        /// Asks
        /// </summary>
        public IDictionary<decimal, decimal> Asks { get; set; } = new SortedList<decimal, decimal>();
    }

    /// <summary>
    /// Order book snapshot
    /// </summary>
    internal class OrderBookSnapshot
    {
        public DateTime? LocalDataTime { get; set; }
        public DateTime? ServerDataTime { get; set; }
        public long? SequenceNumber { get; set; }
        public IDictionary<decimal, decimal> Bids { get; set; } = new SortedList<decimal, decimal>();
        public IDictionary<decimal, decimal> Asks { get; set; } = new SortedList<decimal, decimal>();
    }

    internal class OrderBookChecksum
    {
        public long? SequenceNumber { get; set; }
        public int Checksum { get; set; }
    }
}
