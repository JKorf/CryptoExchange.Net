using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedSpotOrder
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string OrderId { get; set; }
        public CommonOrderType OrderType { get; set; }
        public CommonOrderSide Side { get; set; }
        public CommonOrderStatus Status { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuantityFilled { get; set; }
        public decimal? QuoteQuantity { get; set; }
        public decimal? QuoteQuantityFilled { get; set; }
        public decimal? Price { get; set; }
        public decimal? AveragePrice { get; set; }
        public string? ClientOrderId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
