using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedTrade
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }

        public SharedTrade(decimal quantity, decimal price, DateTime timestamp)
        {
            Quantity = quantity;
            Price = price;
            Timestamp = timestamp;
        }
    }
}
