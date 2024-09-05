using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetPositionModeRequest
    {
        public ApiType ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

        public GetPositionModeRequest(ApiType apiType, SharedSymbol? symbol)
        {
            ApiType = apiType;
            Symbol = symbol;
        }
    }
}
