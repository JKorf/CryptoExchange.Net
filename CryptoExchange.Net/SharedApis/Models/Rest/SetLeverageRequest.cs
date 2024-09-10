using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record SetLeverageRequest : SharedSymbolRequest
    {
        public decimal Leverage { get; set; }
        public SharedPositionSide? Side { get; set; }
        public SharedMarginMode? MarginMode { get; set; }

        public SetLeverageRequest(SharedSymbol symbol, decimal leverage, SharedPositionSide? side = null, SharedMarginMode? mode = null) : base(symbol)
        {
            Leverage = leverage;
            Side = side;
            MarginMode = mode;
        }
    }
}
