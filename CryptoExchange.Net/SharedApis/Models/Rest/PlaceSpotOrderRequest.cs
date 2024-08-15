using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record PlaceSpotOrderRequest : SharedSymbolRequest
    {
        public SharedOrderType OrderType { get; set; }
        public SharedOrderSide Side { get; set; }
        public SharedTimeInForce? TimeInForce { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuoteQuantity { get; set; }
        public decimal? Price { get; set; }
        public string? ClientOrderId { get; set; }

        public PlaceSpotOrderRequest(string baseAsset, string quoteAsset, SharedOrderType orderType, SharedOrderSide side) : base(baseAsset, quoteAsset)
        {
            OrderType = orderType;
            Side = side;
        }

        public PlaceSpotOrderRequest(string symbol, SharedOrderType orderType, SharedOrderSide side) : base(symbol)
        {
            OrderType = orderType;
            Side = side;
        }
    }
}
