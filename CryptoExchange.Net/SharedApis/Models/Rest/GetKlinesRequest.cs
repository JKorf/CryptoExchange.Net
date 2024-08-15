using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetKlinesRequest : SharedRequest
    {
        public string? BaseAsset { get; set; }
        public string? QuoteAsset { get; set; }
        public string? Symbol { get; set; }

        public SharedKlineInterval Interval { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetKlinesRequest(string baseAsset, string quoteAsset, SharedKlineInterval interval)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            Interval = interval;
        }

        public GetKlinesRequest(string symbol, SharedKlineInterval interval)
        {
            Symbol = symbol;
            Interval = interval;
        }

        public string GetSymbol(Func<string, string, ApiType?, string> format)
        {
            if (!string.IsNullOrEmpty(Symbol))
                return Symbol;

            return format(BaseAsset, QuoteAsset, ApiType);
        }
    }
}
