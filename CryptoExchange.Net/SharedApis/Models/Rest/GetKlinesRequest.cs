using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetKlinesRequest : SharedSymbolRequest
    {
        public SharedKlineInterval Interval { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetKlinesRequest(string baseAsset, string quoteAsset, SharedKlineInterval interval) : base(baseAsset, quoteAsset)
        {
            Interval = interval;
        }

        public GetKlinesRequest(string symbol, SharedKlineInterval interval) : base(symbol)
        {
            Interval = interval;
        }
    }
}
