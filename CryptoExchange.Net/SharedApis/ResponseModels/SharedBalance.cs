using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedBalance
    {
        public string Asset { get; set; }
        public decimal Available { get; set; }
        public decimal Total { get; set; }
    }
}
