using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot order
    /// </summary>
    public class PlaceSpotOrderOptions : EndpointOptions<PlaceSpotOrderRequest, ISpotOrderRestClient>
    {

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceSpotOrderOptions(string exchange) : base(exchange, true)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceSpotOrderRequest request,
            ISpotOrderRestClient client)
        {
            if (request.OrderType == SharedOrderType.Other)
                throw new ArgumentException("OrderType can't be `Other`", nameof(request.OrderType));

            if (!client.SpotSupportedOrderTypes.Contains(request.OrderType))
                return ArgumentError.Invalid(nameof(PlaceSpotOrderRequest.OrderType), "Order type not supported");

            if (request.TimeInForce != null && !client.SpotSupportedTimeInForce.Contains(request.TimeInForce.Value))
                return ArgumentError.Invalid(nameof(PlaceSpotOrderRequest.TimeInForce), "Order time in force not supported");

            var quantityError = client.SpotSupportedOrderQuantity.Validate(request.Side, request.OrderType, request.Quantity);
            if (quantityError != null)
                return quantityError;

            return base.ValidateRequest(request, client);
        }
    }
}
