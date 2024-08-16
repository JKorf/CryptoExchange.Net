using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetWithdrawalsRequest
    {
        public string? Asset { get; set; }
        public RequestFilter? Filter { get; set; }
    }
}
