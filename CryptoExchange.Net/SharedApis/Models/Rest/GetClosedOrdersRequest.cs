using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetClosedOrdersRequest : SharedRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetClosedOrdersRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}
