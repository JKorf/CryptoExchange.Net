using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetSymbolsRequest : SharedRequest
    {
        public ApiType? ApiType { get; set; }

        public GetSymbolsRequest(ApiType? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ApiType = apiType;
        }
    }
}
