using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetSpotOpenOrdersRequest : SharedRequest
    {
        public string? BaseAsset { get; set; }
        public string? QuoteAsset { get; set; }
    }
}
