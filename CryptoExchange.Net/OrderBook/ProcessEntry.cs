using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.OrderBook
{
    public class ProcessEntry
    {
        public ISymbolOrderBookEntry Entry { get; set; }
        public OrderBookEntryType Type { get; set; }

        public ProcessEntry(OrderBookEntryType type, ISymbolOrderBookEntry entry)
        {
            Type = type;
            Entry = entry;
        }
    }
}
