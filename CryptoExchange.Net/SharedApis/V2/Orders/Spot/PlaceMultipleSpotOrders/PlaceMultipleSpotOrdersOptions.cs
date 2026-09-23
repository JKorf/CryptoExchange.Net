using CryptoExchange.Net.Objects;
using System;
using System.Collections;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing multiple new spot orders
    /// </summary>
    public class PlaceMultipleSpotOrdersOptions : CapabilityOptions<PlaceMultipleSpotOrdersRequest, IPlaceMultipleSpotOrders>
    {
        /// <inheritdoc />
        public override string Description => "Place multiple new spot orders";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<PlaceMultipleSpotOrdersRequest>.Required(x => x.Orders, "The orders to place", []),
        };


        /// <summary>
        /// ctor
        /// </summary>
        public PlaceMultipleSpotOrdersOptions(string exchange)
            : base(exchange, true, nameof(IPlaceMultipleSpotOrders.PlaceMultipleSpotOrdersAsync), _defaultParameterRules, SharedTradingModeSets.Spot)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceMultipleSpotOrdersRequest request,
            IPlaceMultipleSpotOrders client)
        {
            var error = base.ValidateRequest(request, client);
            if (error != null)
                return error;

            if (request.Orders.Length > client.MaxSpotOrdersPerRequest)
                return ArgumentError.Invalid(nameof(PlaceMultipleSpotOrdersRequest.Orders), $"Too many orders, max {client.MaxSpotOrdersPerRequest}");

            if (request.Orders.Length == 0)
                return ArgumentError.Invalid(nameof(PlaceMultipleSpotOrdersRequest.Orders), "No orders provided");

            if (request.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.TradingMode} is not supported, should be Spot");

            if (!client.PlaceMultipleSpotOrdersAllowsMultipleSymbols)
            {
                var firstSymbol = request.Orders[0].Symbol;
                if (request.Orders.Any(order => order.Symbol!.BaseAsset != firstSymbol!.BaseAsset
                        || order.Symbol.QuoteAsset != firstSymbol.QuoteAsset
                        || order.Symbol.TradingMode != firstSymbol.TradingMode))
                {
                    return ArgumentError.Invalid(nameof(PlaceSpotOrderRequest.Symbol), "All orders in the request should have matching symbols");
                }
            }

            foreach (var order in request.Orders)
            {
                if (order.OrderType == SharedOrderType.Other)
                    throw new ArgumentException("OrderType can't be `Other`", nameof(order.OrderType));

                if (!client.SpotSupportedOrderTypes.Contains(order.OrderType))
                    return ArgumentError.Invalid(nameof(PlaceSpotOrderRequest.OrderType), "Order type not supported");

                if (order.TimeInForce != null && !client.SpotSupportedTimeInForce.Contains(order.TimeInForce.Value))
                    return ArgumentError.Invalid(nameof(PlaceSpotOrderRequest.TimeInForce), "Order time in force not supported");

                var quantityError = client.SpotSupportedOrderQuantity.Validate(order.Side, order.OrderType, order.Quantity);
                if (quantityError != null)
                    return quantityError;
            }

            

            return null;
        }
    }
}
