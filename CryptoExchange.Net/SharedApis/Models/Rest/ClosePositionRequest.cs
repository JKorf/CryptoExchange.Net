using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record ClosePositionRequest : SharedSymbolRequest
    {
        public SharedPositionSide PositionSide { get; set; }
        public SharedMarginMode? MarginMode { get; set; }

        public ClosePositionRequest(SharedSymbol symbol, SharedPositionSide side) : base(symbol)
        {
            PositionSide = side;
        }
    }
}
