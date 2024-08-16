using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetSpotClosedOrdersRequest : SharedSymbolRequest
    {
        public RequestFilter? Filter { get; set; }

        public GetSpotClosedOrdersRequest(string baseAsset, string quoteAsset) : base(baseAsset, quoteAsset)
        {
        }

        public GetSpotClosedOrdersRequest(string symbol) : base(symbol)
        {
        }
    }
}
