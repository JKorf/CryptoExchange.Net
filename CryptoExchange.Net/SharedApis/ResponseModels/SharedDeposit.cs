using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedDeposit
    {
        public string Asset { get; set; }
        public decimal Quantity { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
