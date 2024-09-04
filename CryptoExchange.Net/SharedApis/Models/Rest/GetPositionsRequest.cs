using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetPositionsRequest
    {
        public ApiType ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

        public GetPositionsRequest(ApiType apiType)
        {
            ApiType = apiType;
        }
    }
}
