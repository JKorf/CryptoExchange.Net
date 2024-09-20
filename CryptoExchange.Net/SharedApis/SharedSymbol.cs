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
        public TradingMode ApiType { get; set; }
        public DateTime? DeliverTime { get; set; }

        public SharedSymbol(TradingMode apiType, string baseAsset, string quoteAsset, DateTime? deliverTime = null)
        {
            ApiType = apiType;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            DeliverTime = deliverTime;
        }

        public SharedSymbol(TradingMode apiType, string baseAsset, string quoteAsset, string symbolName)
        {
            ApiType = apiType;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            SymbolName = symbolName;
        }

        public string GetSymbol(Func<string, string, TradingMode, DateTime?, string> format)
        {
            if (!string.IsNullOrEmpty(SymbolName))
                return SymbolName;

            return format(BaseAsset, QuoteAsset, ApiType, DeliverTime);
        }
    }
}
