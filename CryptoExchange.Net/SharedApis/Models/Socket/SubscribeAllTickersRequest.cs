using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeAllTickersRequest : SharedRequest
    {
        public ApiType? ApiType { get; set; }

        public SubscribeAllTickersRequest(ApiType? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters) { }
    }
}
