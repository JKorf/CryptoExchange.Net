using CryptoExchange.Net.CommonObjects;
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

        public GetOrderTradesRequest(string baseAsset, string quoteAsset, string orderId) : base(baseAsset, quoteAsset)
        {
            OrderId = orderId;
        }

        public GetOrderTradesRequest(string symbol, string orderId) : base(symbol)
        {
            OrderId = orderId;
        }
    }
}
