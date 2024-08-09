using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record FuturesOrderRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public SharedOrderSide Side { get; set; }
        public SharedOrderType OrderType { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuoteQuantity { get; set; }
        public decimal? Price { get; set; }
        public string? ClientOrderId { get; set; }

        // Other props?
        // Leverage?
        // Long/Short?
    }
}
