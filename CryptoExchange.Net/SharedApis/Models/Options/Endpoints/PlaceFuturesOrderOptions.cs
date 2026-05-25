using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new futures order
    /// </summary>
    public class PlaceFuturesOrderOptions : EndpointOptions<PlaceFuturesOrderRequest, IFuturesOrderRestClient>
    {
        /// <summary>
        /// Whether or not the API supports setting take profit / stop loss with the order
        /// </summary>
        public bool SupportsTpSl { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceFuturesOrderOptions(string exchange, bool supportsTpSl) : base(exchange, true)
        {
            SupportsTpSl = supportsTpSl;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceFuturesOrderRequest request,
            IFuturesOrderRestClient client
            )
        {
            if (!SupportsTpSl && (request.StopLossPrice != null || request.TakeProfitPrice != null))
                return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.StopLossPrice) + " / " + nameof(PlaceFuturesOrderRequest.TakeProfitPrice), "Tp/Sl parameters not supported");

            if (request.OrderType == SharedOrderType.Other)
                throw new ArgumentException("OrderType can't be `Other`", nameof(request.OrderType));

            if (!client.FuturesSupportedOrderTypes.Contains(request.OrderType))
                return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.OrderType), "Order type not supported");

            if (request.TimeInForce != null && !client.FuturesSupportedTimeInForce.Contains(request.TimeInForce.Value))
                return ArgumentError.Invalid(nameof(PlaceFuturesOrderRequest.TimeInForce), "Order time in force not supported");

            var quantityError = client.FuturesSupportedOrderQuantity.Validate(request.Side, request.OrderType, request.Quantity);
            if (quantityError != null)
                return quantityError;

            return base.ValidateRequest(request, client);
        }

    }
}
