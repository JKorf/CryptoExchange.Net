using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedUserTrade
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal? Fee { get; set; }
        public string? FeeAsset { get; set; }
        public SharedRole Role { get; set; }
        public DateTime Timestamp { get; set; }

        public SharedUserTrade(string id, string orderId, decimal quantity, decimal price, SharedRole role, DateTime timestamp)
        {
            Id = id;
            OrderId = orderId;
            Quantity = quantity;
            Price = price;
            Role = role;
            Timestamp = timestamp;
        }
    }
}
