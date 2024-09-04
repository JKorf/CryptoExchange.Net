using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
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

        public GetUserTradesRequest(ApiType apiType, SharedSymbol symbol) : base(symbol, apiType)
        {
        }
    }
}
