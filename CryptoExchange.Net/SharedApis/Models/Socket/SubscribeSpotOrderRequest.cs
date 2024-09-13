using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeSpotOrderRequest : SharedRequest
    {
        public string? ListenKey { get; set; }

        public SubscribeSpotOrderRequest(string? listenKey = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ListenKey = listenKey;
        }
    }
}
