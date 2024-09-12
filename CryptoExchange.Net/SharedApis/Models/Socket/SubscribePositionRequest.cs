using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribePositionRequest
    {
        public string? ListenKey { get; set; }

        public SubscribePositionRequest(string? listenKey = null)
        {
            ListenKey = listenKey;
        }
    }
}
