using CryptoExchange.Net.Interfaces;
using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Order book info
    /// </summary>
    public record SharedOrderBook
    {
        /// <summary>
        /// Asks list
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Asks { get; set; }
        /// <summary>
        /// Bids list
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Bids { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedOrderBook(IEnumerable<ISymbolOrderBookEntry> asks, IEnumerable<ISymbolOrderBookEntry> bids)
        {
            Asks = asks;
            Bids = bids;
        }
    }

}
