using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record StopListenKeyRequest : SharedRequest
    {
        public string ListenKey { get; set; }
        public TradingMode? ApiType { get; set; }

        public StopListenKeyRequest(string listenKey, TradingMode? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ListenKey = listenKey;
            ApiType = apiType;
        }
    }
}
