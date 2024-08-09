using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record OrderBookRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public int? Limit { get; set; }
    }
}
