using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new futures order
    /// </summary>
    public class PlaceFuturesOrderOptions : EndpointOptions<PlaceFuturesOrderRequest>
    {
        /// <summary>
        /// Whether or not the API supports setting take profit / stop loss with the order
        /// </summary>
        public bool SupportsTpSl { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceFuturesOrderOptions(bool supportsTpSl) : base(true)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public Error? ValidateRequest(
            string exchange,
            PlaceFuturesOrderRequest request,
            TradingMode? tradingMode,
            TradingMode[] supportedApiTypes,
            SharedOrderType[] supportedOrderTypes,
            SharedTimeInForce[] supportedTimeInForce,
            SharedQuantitySupport quantitySupport)
        {
            if (!SupportsTpSl && (request.StopLossPrice != null || request.TakeProfitPrice != null))
                return new ArgumentError("Tp/Sl parameters not supported");

            if (request.OrderType == SharedOrderType.Other)
                throw new ArgumentException("OrderType can't be `Other`", nameof(request.OrderType));

            if (!supportedOrderTypes.Contains(request.OrderType))
                return new ArgumentError("Order type not supported");

            if (request.TimeInForce != null && !supportedTimeInForce.Contains(request.TimeInForce.Value))
                return new ArgumentError("Order time in force not supported");

            var quantityError = quantitySupport.Validate(request.Side, request.OrderType, request.Quantity);
            if (quantityError != null)
                return quantityError;

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

    }
}
