using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedUserTrade
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public SharedRole Role { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
