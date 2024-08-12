using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetKlinesRequest : SharedRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetKlinesRequest(string baseAsset, string quoteAsset, TimeSpan interval)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            Interval = interval;
        }
    }
}
