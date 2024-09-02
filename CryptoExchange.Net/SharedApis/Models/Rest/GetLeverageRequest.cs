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

        public GetLeverageRequest(SharedSymbol symbol, SharedPositionSide side) : base(symbol)
        {
            Side = side;
        }
    }
}
