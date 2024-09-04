using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetOrderTradesRequest : SharedSymbolRequest
    {
        public string OrderId { get; set; }

        public GetOrderTradesRequest(SharedSymbol symbol, string orderId, ApiType apiType) : base(symbol, apiType)
        {
            OrderId = orderId;
        }
    }
}
