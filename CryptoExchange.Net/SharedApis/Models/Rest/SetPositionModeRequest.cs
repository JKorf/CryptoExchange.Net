using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record SetPositionModeRequest
    {
        public ApiType ApiType { get; set; }
        public SharedPositionMode Mode { get; set; }

        public SharedSymbol? Symbol { get; set; }
        public SetPositionModeRequest(ApiType apiType, SharedPositionMode mode, SharedSymbol? symbol)
        {
            ApiType = apiType;
            Mode = mode;
            Symbol = symbol;
        }
    }
}
