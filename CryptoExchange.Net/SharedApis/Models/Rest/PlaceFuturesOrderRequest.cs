using CryptoExchange.Net.CommonObjects;
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
        public decimal? Quantity { get; set; }
        public decimal? QuoteQuantity { get; set; }
        public decimal? Price { get; set; }
        public string? ClientOrderId { get; set; }

        // Other props?
        // Leverage?
        // Long/Short?


        public PlaceFuturesOrderRequest(string baseAsset, string quoteAsset) : base(baseAsset, quoteAsset)
        {
        }

        public PlaceFuturesOrderRequest(string symbol) : base(symbol)
        {
        }
    }
}
