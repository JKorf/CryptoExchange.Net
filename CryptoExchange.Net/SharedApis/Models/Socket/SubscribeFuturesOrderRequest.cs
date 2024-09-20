using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeFuturesOrderRequest : SharedRequest
    {
        public TradingMode? ApiType { get; set; }
        public string? ListenKey { get; set; }

        public SubscribeFuturesOrderRequest(string? listenKey = null, TradingMode? apiType = null, ExchangeParameters? exchangeParameters = null): base(exchangeParameters)
        {
            ListenKey = listenKey;
        }
    }
}
