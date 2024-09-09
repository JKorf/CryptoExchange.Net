using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public record SharedSymbol
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string SymbolName { get; set; }
        public DateTime? DeliverTime { get; set; }

        public SharedSymbol(string baseAsset, string quoteAsset, DateTime? deliverTime = null)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            DeliverTime = deliverTime;
        }

        public SharedSymbol(string baseAsset, string quoteAsset, string symbolName)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            SymbolName = symbolName;
        }

        public string GetSymbol(Func<string, string, DateTime?, string> format)
        {
            if (!string.IsNullOrEmpty(SymbolName))
                return SymbolName;

            return format(BaseAsset, QuoteAsset, DeliverTime);
        }
    }
}
