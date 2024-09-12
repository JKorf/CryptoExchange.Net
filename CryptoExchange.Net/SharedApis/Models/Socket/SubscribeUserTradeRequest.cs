using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeUserTradeRequest
    {
        public string? ListenKey { get; set; }

        public SubscribeUserTradeRequest(string? listenKey = null)
        {
            ListenKey = listenKey;
        }
    }
}
