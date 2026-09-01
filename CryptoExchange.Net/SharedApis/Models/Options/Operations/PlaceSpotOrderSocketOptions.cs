using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot order
    /// </summary>
    public class PlaceSpotOrderSocketOptions : CapabilityOptions<PlaceSpotOrderRequest, IPlaceSpotOrderSocket>
    {
        /// <inheritdoc />
        public override string Description => "Place a new spot order over a socket connection";


        /// <summary>
        /// ctor
        /// </summary>
        public PlaceSpotOrderSocketOptions(string exchange) : base(exchange, true, nameof(IPlaceSpotOrderSocket.PlaceSpotOrderAsync))
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceSpotOrderRequest request,
            IPlaceSpotOrderSocket client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

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
