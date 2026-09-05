using CryptoExchange.Net.Objects;
using System;
using System.Collections;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot order
    /// </summary>
    public class PlaceSpotOrderOptions : CapabilityOptions<PlaceSpotOrderRequest, IPlaceSpotOrder>
    {
        /// <inheritdoc />
        public override string Description => "Place a new spot order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<PlaceSpotOrderRequest>.Required(x => x.Symbol, "Symbol", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<PlaceSpotOrderRequest>.Required(x => x.OrderType, "Order type", SharedOrderType.Limit),
            RequestParameterRule<PlaceSpotOrderRequest>.Required(x => x.Side, "Order side", SharedOrderSide.Buy),
            RequestParameterRule<PlaceSpotOrderRequest>.Optional(x => x.TimeInForce, "Time in force", SharedTimeInForce.GoodTillCanceled),
            RequestParameterRule<PlaceSpotOrderRequest>.Optional(x => x.Quantity, "Order quantity", SharedQuantity.Base(0.1m)),
            RequestParameterRule<PlaceSpotOrderRequest>.Optional(x => x.Price, "Order price", 1m),
            RequestParameterRule<PlaceSpotOrderRequest>.Optional(x => x.ClientOrderId, "Client order id", "123")
        };


        /// <summary>
        /// ctor
        /// </summary>
        public PlaceSpotOrderOptions(string exchange)
            : base(exchange, true, nameof(IPlaceSpotOrder.PlaceSpotOrderAsync), _defaultParameterRules)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceSpotOrderRequest request,
            IPlaceSpotOrder client)
        {
            var error = base.ValidateRequest(request, client);
            if (error != null)
                return error;

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

            return null;
        }
    }
}
