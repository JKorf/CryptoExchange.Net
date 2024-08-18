using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record PlaceSpotOrderOptions
    {
        public IEnumerable<SharedOrderType> SupportedOrderType { get; }
        public IEnumerable<SharedTimeInForce> SupportedTimeInForce { get; }
        public SharedQuantitySupport OrderQuantitySupport { get; }

        public PlaceSpotOrderOptions(IEnumerable<SharedOrderType> orderTypes, IEnumerable<SharedTimeInForce> timeInForces, SharedQuantitySupport quantitySupport)
        {
            SupportedOrderType = orderTypes;
            SupportedTimeInForce = timeInForces;
            OrderQuantitySupport = quantitySupport;
        }

        public Error? Validate(PlaceSpotOrderRequest request)
        {
            if (!SupportedOrderType.Contains(request.OrderType))
                return new ArgumentError("Order type not supported");

            if (request.TimeInForce != null && !SupportedTimeInForce.Contains(request.TimeInForce.Value))
                return new ArgumentError("Order time in force not supported");

            var quantityError = OrderQuantitySupport.Validate(request.Side, request.OrderType, request.Quantity, request.QuoteQuantity);
            if (quantityError != null)
                return quantityError;

            return null;
        }
    }
}
