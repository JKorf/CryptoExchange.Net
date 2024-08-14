using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedUserTrade
    {
        public string Symbol { get; set; }
        public string Id { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }

        public string? OrderId { get; set; }
        public decimal? Fee { get; set; }
        public string? FeeAsset { get; set; }
        public SharedRole? Role { get; set; }

        public SharedUserTrade(string symbol, string orderId, string id, decimal quantity, decimal price, DateTime timestamp)
        {
            Symbol = symbol;
            OrderId = orderId;
            Id = id;
            Quantity = quantity;
            Price = price;
            Timestamp = timestamp;
        }
    }
}
