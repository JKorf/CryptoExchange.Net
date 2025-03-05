﻿using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.OrderBook
{
    internal class ProcessQueueItem
    {
        public long StartUpdateId { get; set; }
        public long EndUpdateId { get; set; }
        public ISymbolOrderBookEntry[] Bids { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
        public ISymbolOrderBookEntry[] Asks { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
    }

    internal class InitialOrderBookItem
    {
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
