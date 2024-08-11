using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record GetOrderRequest : SharedRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string OrderId { get; set; }

        public GetOrderRequest(string baseAsset, string quoteAsset, string orderId)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            OrderId = orderId;
        }
    }
}
