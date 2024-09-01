using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedOpenInterest
    {
        public decimal OpenInterest { get; set; }

        public SharedOpenInterest(decimal openInterest)
        {
            OpenInterest = openInterest;
        }
    }
}
