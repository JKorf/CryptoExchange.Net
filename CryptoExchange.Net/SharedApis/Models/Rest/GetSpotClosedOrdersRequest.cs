using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetSpotClosedOrdersRequest : SharedSymbolRequest
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetSpotClosedOrdersRequest(string baseAsset, string quoteAsset) : base(baseAsset, quoteAsset)
        {
        }

        public GetSpotClosedOrdersRequest(string symbol) : base(symbol)
        {
        }
    }
}
