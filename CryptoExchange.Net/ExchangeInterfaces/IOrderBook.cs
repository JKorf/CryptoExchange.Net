using System.Collections.Generic;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common order book
    /// </summary>
    public interface ICommonOrderBook
    {
        /// <summary>
        /// Bids
        /// </summary>
        IEnumerable<ISymbolOrderBookEntry> CommonBids { get; }
        /// <summary>
        /// Asks
        /// </summary>
        IEnumerable<ISymbolOrderBookEntry> CommonAsks { get; }
    }
}
