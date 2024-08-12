using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedSpotOrder
    {
        public string Symbol { get; set; }
        public string OrderId { get; set; }
        public SharedOrderType OrderType { get; set; }
        public SharedOrderSide Side { get; set; }
        public SharedOrderStatus Status { get; set; }
        public SharedTimeInForce TimeInForce { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuantityFilled { get; set; }
        public decimal? QuoteQuantity { get; set; }
        public decimal? QuoteQuantityFilled { get; set; }
        public decimal? Price { get; set; }
        public decimal? AveragePrice { get; set; }
        public string? ClientOrderId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }

        public SharedSpotOrder(
            string symbol,
            string orderId,
            SharedOrderType orderType,
            SharedOrderSide orderSide,
            SharedOrderStatus orderStatus,
            DateTime createTime)
        {
            Symbol = symbol;
            OrderId = orderId;
            OrderType = orderType;
            Side = orderSide;
            Status = orderStatus;
            CreateTime = createTime;
        }
    }
}
