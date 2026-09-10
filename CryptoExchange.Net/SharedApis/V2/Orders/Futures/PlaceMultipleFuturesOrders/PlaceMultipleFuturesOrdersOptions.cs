using CryptoExchange.Net.Objects;
using System;
using System.Collections;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing multiple new futures orders
    /// </summary>
    public class PlaceMultipleFuturesOrdersOptions : CapabilityOptions<PlaceMultipleFuturesOrdersRequest, IPlaceMultipleFuturesOrders>
    {
        /// <inheritdoc />
        public override string Description => "Place multiple new futures orders";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<PlaceMultipleSpotOrdersRequest>.Required(x => x.Orders, "The orders to place", []),
        };


        /// <summary>
        /// ctor
        /// </summary>
        public PlaceMultipleFuturesOrdersOptions(string exchange)
            : base(exchange, true, nameof(IPlaceMultipleFuturesOrders.PlaceMultipleFuturesOrdersAsync), _defaultParameterRules, SharedTradingModeSets.Futures)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceMultipleFuturesOrdersRequest request,
            IPlaceMultipleFuturesOrders client)
        {
            var error = base.ValidateRequest(request, client);
            if (error != null)
                return error;

            if (request.Orders.Length > client.MaxFuturesOrdersPerRequest)
                return ArgumentError.Invalid(nameof(PlaceMultipleFuturesOrdersRequest.Orders), $"Too many orders, max {client.MaxFuturesOrdersPerRequest}");

            if (request.Orders.Length == 0)
                return ArgumentError.Invalid(nameof(PlaceMultipleFuturesOrdersRequest.Orders), "No orders provided");

            if (!client.PlaceMultipleFuturesOrdersAllowsMultipleSymbols)
            {
                var firstSymbol = request.Orders[0].Symbol;
                if (request.Orders.Any(order => order.Symbol!.BaseAsset != firstSymbol!.BaseAsset
                        || order.Symbol.QuoteAsset != firstSymbol.QuoteAsset
                        || order.Symbol.TradingMode != firstSymbol.TradingMode))
                {
                    return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.Symbol), "All orders in the request should have matching symbols");
                }
            }

            if (!request.Orders.All(x => x.TradingMode == request.TradingMode))
                return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.TradingMode), "All orders in the request should have matching trading modes");

            foreach (var order in request.Orders)
            {
                if (order.OrderType == SharedOrderType.Other)
                    throw new ArgumentException("OrderType can't be `Other`", nameof(order.OrderType));

                if (!client.FuturesSupportedOrderTypes.Contains(order.OrderType))
                    return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.OrderType), "Order type not supported");

                if (order.TimeInForce != null && !client.FuturesSupportedTimeInForce.Contains(order.TimeInForce.Value))
                    return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.TimeInForce), "Order time in force not supported");

                var quantityError = client.FuturesSupportedOrderQuantity.Validate(order.Side, order.OrderType, order.Quantity);
                if (quantityError != null)
                    return quantityError;
            }
            
            return null;
        }
    }
}
