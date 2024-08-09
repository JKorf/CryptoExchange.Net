using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedWithdrawal
    {
        public string Asset { get; set; }
        public string Address { get; set; }
        public decimal Quantity { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
