using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record GetOpenOrdersRequest : SharedRequest
    {
        public string? BaseAsset { get; set; }
        public string? QuoteAsset { get; set; }
    }
}
