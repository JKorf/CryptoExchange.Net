using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetOrderBookRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public int? Limit { get; set; }

        public GetOrderBookRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}
