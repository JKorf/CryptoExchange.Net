using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetOrderRequest : SharedSymbolRequest
    {
        public string OrderId { get; set; }

        public GetOrderRequest(SharedSymbol symbol, string orderId) : base(symbol)
        {
            OrderId = orderId;
        }
    }
}
