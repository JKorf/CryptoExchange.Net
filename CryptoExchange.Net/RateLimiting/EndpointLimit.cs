using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.RateLimiting
{
    public struct EndpointLimit
    {
        public int Limit { get; set; }
        public TimeSpan Period { get; set; }
    }
}
