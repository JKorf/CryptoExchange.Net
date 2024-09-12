using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record SetPositionModeRequest
    {
        public SharedSymbol? Symbol { get; set; }
        public ApiType? ApiType { get; set; }
        public SharedPositionMode Mode { get; set; }

        public SetPositionModeRequest(ApiType apiType, SharedPositionMode mode)
        {
            ApiType = apiType;
            Mode = mode;
        }
        
        public SetPositionModeRequest(SharedSymbol? symbol, SharedPositionMode mode)
        {
            Mode = mode;
            Symbol = symbol;
        }
    }
}
