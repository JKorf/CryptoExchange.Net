using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.OrderBook.V2
{
    /// <summary>
    /// Book snapshot
    /// </summary>
    /// <param name="sequenceNumber">Sequence number</param>
    /// <param name="asks">Asks</param>
    /// <param name="bids">Bids</param>
    public class BookSnapshot(long sequenceNumber, IReadOnlyDictionary<decimal, decimal> asks, IReadOnlyDictionary<decimal, decimal> bids)
    {
        /// <summary>
        /// Sequence number
        /// </summary>
        public long SequenceNumber { get; } = sequenceNumber;
        /// <summary>
        /// Asks
        /// </summary>
        public IReadOnlyDictionary<decimal, decimal> Asks { get; } = asks;
        /// <summary>
        /// Bids
        /// </summary>
        public IReadOnlyDictionary<decimal, decimal> Bids { get; } = bids;
    }

}
