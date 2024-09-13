using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetAssetsRequest : SharedRequest
    {
        public ApiType? ApiType { get; set; }

        public GetAssetsRequest(ApiType? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ApiType = apiType;
        }
    }
}
