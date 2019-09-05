using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.OrderBook
{
    /// <summary>
    /// Process entry for order book
    /// </summary>
    public class ProcessEntry
    {
        /// <summary>
        /// The entry
        /// </summary>
        public ISymbolOrderBookEntry Entry { get; set; }
        /// <summary>
        /// The type
        /// </summary>
        public OrderBookEntryType Type { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entry"></param>
        public ProcessEntry(OrderBookEntryType type, ISymbolOrderBookEntry entry)
        {
            Type = type;
            Entry = entry;
        }
    }
}
