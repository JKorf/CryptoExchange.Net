using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeBalancesRequest
    {
        public string? ListenKey { get; set; }

        public SubscribeBalancesRequest(string? listenKey = null)
        {
            ListenKey = listenKey;
        }
    }
}
