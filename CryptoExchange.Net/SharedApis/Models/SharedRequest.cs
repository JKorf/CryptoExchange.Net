using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedSymbolRequest
    {
        public ApiType? ApiType { get; set; }
        public string? BaseAsset { get; set; }
        public string? QuoteAsset { get; set; }
        public string? Symbol { get; set; }

        public SharedSymbolRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }

        public SharedSymbolRequest(string symbol)
        {
            Symbol = symbol;
        }

        public string GetSymbol(Func<string, string, ApiType?, string> format)
        {
            if (!string.IsNullOrEmpty(Symbol))
                return Symbol;

            return format(BaseAsset, QuoteAsset, ApiType);
        }
    }
}
