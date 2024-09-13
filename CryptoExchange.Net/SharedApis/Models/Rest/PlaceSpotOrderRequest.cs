using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
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

        public PlaceSpotOrderRequest(
            SharedSymbol symbol,
            SharedOrderSide side,
            SharedOrderType orderType,
            decimal? quantity = null,
            decimal? quoteQuantity = null,
            decimal? price = null,
            SharedTimeInForce? timeInForce = null,
            string? clientOrderId = null,
            ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            OrderType = orderType;
            Side = side;
            Quantity = quantity;
            QuoteQuantity = quoteQuantity;
            Price = price;
            TimeInForce = timeInForce;
            ClientOrderId = clientOrderId;
        }
    }
}
