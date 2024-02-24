using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Order book data
    /// </summary>
    public class OrderBook: BaseCommonObject
    {
        /// <summary>
        /// List of bids
        /// </summary>
        public IEnumerable<OrderBookEntry> Bids { get; set; } = Array.Empty<OrderBookEntry>();
        /// <summary>
        /// List of asks
        /// </summary>
        public IEnumerable<OrderBookEntry> Asks { get; set; } = Array.Empty<OrderBookEntry>();
    }
}
