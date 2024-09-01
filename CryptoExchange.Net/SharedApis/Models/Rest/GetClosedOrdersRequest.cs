using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetClosedOrdersRequest : SharedSymbolRequest
    {
        public RequestFilter? Filter { get; set; }

        public GetClosedOrdersRequest(SharedSymbol symbol) : base(symbol)
        {
        }
    }
}
