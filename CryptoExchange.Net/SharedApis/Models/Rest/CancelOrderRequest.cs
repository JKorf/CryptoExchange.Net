using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record CancelOrderRequest : SharedSymbolRequest
    {
        public string OrderId { get; set; }

        public CancelOrderRequest(SharedSymbol symbol, string orderId) : base(symbol)
        {
            OrderId = orderId;
        }
    }
}
