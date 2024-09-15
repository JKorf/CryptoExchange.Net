using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetOpenOrdersRequest : SharedRequest
    {
        public ApiType? ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

        public GetOpenOrdersRequest(ApiType? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ApiType = apiType;
        }

        public GetOpenOrdersRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Symbol = symbol;
        }
    }
}
