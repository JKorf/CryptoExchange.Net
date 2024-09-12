using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeSpotOrderRequest
    {
        public string? ListenKey { get; set; }

        public SubscribeSpotOrderRequest(string? listenKey = null)
        {
            ListenKey = listenKey;
        }
    }
}
