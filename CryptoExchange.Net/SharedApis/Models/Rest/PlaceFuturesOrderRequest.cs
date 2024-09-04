using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record PlaceFuturesOrderRequest : SharedSymbolRequest
    {
        public SharedOrderSide Side { get; set; }
        public SharedOrderType OrderType { get; set; }
        public SharedTimeInForce? TimeInForce { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuoteQuantity { get; set; }
        public decimal? Price { get; set; }
        public string? ClientOrderId { get; set; }
        public SharedPositionSide PositionSide { get; set; }


        public SharedMarginMode? MarginMode { get; set; }
        public bool? ReduceOnly { get; set; }

#warning should have note that it might not be applied depending on API
        public decimal? Leverage { get; set; }


        public PlaceFuturesOrderRequest(SharedSymbol symbol, SharedOrderSide side, SharedOrderType type, ApiType apiType) : base(symbol, apiType)
        {
            Side = side;
            OrderType = type;
        }
    }
}
