using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetOrderTradesRequest : SharedRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string OrderId { get; set; }

        public GetOrderTradesRequest(string baseAsset, string quoteAsset, string orderId)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            OrderId = orderId;
        }
    }
}
