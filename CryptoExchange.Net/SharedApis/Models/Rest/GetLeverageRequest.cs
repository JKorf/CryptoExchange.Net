using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetLeverageRequest : SharedSymbolRequest
    {
        public SharedPositionSide Side { get; set; }

        public SharedMarginMode? MarginMode { get; set; }

        public GetLeverageRequest(SharedSymbol symbol, SharedPositionSide side, ApiType apiType) : base(symbol, apiType)
        {
            Side = side;
        }
    }
}
