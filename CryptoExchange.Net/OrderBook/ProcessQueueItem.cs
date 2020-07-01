using CryptoExchange.Net.Interfaces;
using System.Collections.Generic;

namespace CryptoExchange.Net.OrderBook
{
    internal class ProcessQueueItem
    {
        public long StartUpdateId { get; set; }
        public long EndUpdateId { get; set; }
        public IEnumerable<ISymbolOrderBookEntry> Bids { get; set; } = new List<ISymbolOrderBookEntry>();
        public IEnumerable<ISymbolOrderBookEntry> Asks { get; set; } = new List<ISymbolOrderBookEntry>();
    }

    internal class InitialOrderBookItem
    {
        public long StartUpdateId { get; set; }
        public long EndUpdateId { get; set; }
        public IEnumerable<ISymbolOrderBookEntry> Bids { get; set; } = new List<ISymbolOrderBookEntry>();
        public IEnumerable<ISymbolOrderBookEntry> Asks { get; set; } = new List<ISymbolOrderBookEntry>();
    }

    internal class ChecksumItem
    {
        public int Checksum { get; set; }
    }
}
