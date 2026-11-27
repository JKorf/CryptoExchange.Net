using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.OrderBook
{
    internal class OrderBookUpdate
    {
        public DateTime? LocalDataTime { get; set; }
        public DateTime? ServerDataTime { get; set; }
        public long StartSequenceNumber { get; set; }
        public long EndSequenceNumber { get; set; }
        public ISymbolOrderBookEntry[] Bids { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
        public ISymbolOrderBookEntry[] Asks { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
    }

    internal class OrderBookSnapshot
    {
        public DateTime? LocalDataTime { get; set; }
        public DateTime? ServerDataTime { get; set; }
        public long? SequenceNumber { get; set; }
        public ISymbolOrderBookEntry[] Bids { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
        public ISymbolOrderBookEntry[] Asks { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
    }

    internal class OrderBookChecksum
    {
        public long? SequenceNumber { get; set; }
        public int Checksum { get; set; }
    }
}
