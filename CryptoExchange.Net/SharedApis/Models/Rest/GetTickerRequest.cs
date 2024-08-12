using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetTickerRequest : SharedRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }

        public GetTickerRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}
