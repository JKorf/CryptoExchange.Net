using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeFuturesOrderRequest
    {
        public string? ListenKey { get; set; }

        public SubscribeFuturesOrderRequest(string? listenKey = null)
        {
            ListenKey = listenKey;
        }
    }
}
