using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribePositionRequest: SharedRequest
    {
        public string? ListenKey { get; set; }
        public ApiType? ApiType { get; set; }

        public SubscribePositionRequest(string? listenKey = null, ApiType? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ListenKey = listenKey;
        }
    }
}
