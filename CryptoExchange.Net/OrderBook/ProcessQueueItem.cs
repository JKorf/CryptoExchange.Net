using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.OrderBook
{
    internal class ProcessQueueItem
    {
        public DateTime? LocalDataTime { get; set; }
        public DateTime? ServerDataTime { get; set; }
        public long StartUpdateId { get; set; }
        public long EndUpdateId { get; set; }
        public ISymbolOrderBookEntry[] Bids { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
        public ISymbolOrderBookEntry[] Asks { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
    }

    internal class InitialOrderBookItem
    {
        public DateTime? LocalDataTime { get; set; }
        public DateTime? ServerDataTime { get; set; }
        public long StartUpdateId { get; set; }
        public long EndUpdateId { get; set; }
        public ISymbolOrderBookEntry[] Bids { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
        public ISymbolOrderBookEntry[] Asks { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
    }

    internal class ChecksumItem
    {
        public int Checksum { get; set; }
    }
}
