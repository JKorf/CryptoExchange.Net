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

        public SetLeverageRequest(ApiType apiType, SharedSymbol symbol, decimal leverage) : base(symbol, apiType)
        {
            Leverage = leverage;
        }
    }
}
