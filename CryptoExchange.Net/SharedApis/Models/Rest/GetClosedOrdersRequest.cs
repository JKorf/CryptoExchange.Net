using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetClosedOrdersRequest : SharedSymbolRequest
    {
        public RequestFilter? Filter { get; set; }

        public GetClosedOrdersRequest(SharedSymbol symbol, ApiType apiType) : base(symbol, apiType)
        {
        }
    }
}
