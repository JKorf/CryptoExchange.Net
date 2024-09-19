using CryptoExchange.Net.Objects;
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
        public SharedPositionMode PositionMode { get; set; }
        public SharedPositionSide? PositionSide { get; set; }
        public SharedMarginMode? MarginMode { get; set; }
#warning Quantity is needed when we need to manually place an order
        public decimal? Quantity { get; set; }

        public ClosePositionRequest(
            SharedSymbol symbol,
            SharedPositionMode mode,
            SharedPositionSide? positionSide = null,
            SharedMarginMode? marginMode = null,
            decimal? quantity = null,
            ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            PositionMode = mode;
            PositionSide = positionSide;
            MarginMode = marginMode;
            Quantity = quantity;
        }
    }
}
