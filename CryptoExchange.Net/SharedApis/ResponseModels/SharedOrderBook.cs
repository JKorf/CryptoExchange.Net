using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedOrderBook
    {
        public IEnumerable<ISymbolOrderBookEntry> Asks { get; set; }
        public IEnumerable<ISymbolOrderBookEntry> Bids { get; set; }
    }

}
