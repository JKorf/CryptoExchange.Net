using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Options for placing a new spot order
/// </summary>
public class PlaceSpotOrderOptions : EndpointOptions<PlaceSpotOrderRequest>
{

    /// <summary>
    /// ctor
    /// </summary>
    public PlaceSpotOrderOptions() : base(true)
    {
    }

    /// <summary>
    /// Validate a request
    /// </summary>
    public Error? ValidateRequest(
        string exchange,
        PlaceSpotOrderRequest request,
        TradingMode? tradingMode,
        TradingMode[] supportedApiTypes,
        SharedOrderType[] supportedOrderTypes,
        SharedTimeInForce[] supportedTimeInForce,
        SharedQuantitySupport quantitySupport)
    {
        if (request.OrderType == SharedOrderType.Other)
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            throw new ArgumentException("OrderType can't be `Other`", nameof(request.OrderType));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

        if (!supportedOrderTypes.Contains(request.OrderType))
            return ArgumentError.Invalid(nameof(PlaceSpotOrderRequest.OrderType), "Order type not supported");

        if (request.TimeInForce != null && !supportedTimeInForce.Contains(request.TimeInForce.Value))
            return ArgumentError.Invalid(nameof(PlaceSpotOrderRequest.TimeInForce), "Order time in force not supported");

        var quantityError = quantitySupport.Validate(request.Side, request.OrderType, request.Quantity);
        if (quantityError != null)
            return quantityError;

        return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
    }
}
