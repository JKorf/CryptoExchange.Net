using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetUserTradesRequest : SharedSymbolRequest
    {
        public RequestFilter? Filter { get; set; }

        public GetUserTradesRequest(string baseAsset, string quoteAsset) : base(baseAsset, quoteAsset)
        {
        }

        public GetUserTradesRequest(string symbol) : base(symbol)
        {
        }
    }
}
