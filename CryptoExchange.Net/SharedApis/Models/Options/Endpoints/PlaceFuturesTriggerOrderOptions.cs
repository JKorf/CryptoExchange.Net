﻿using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot trigger order
    /// </summary>
    public class PlaceFuturesTriggerOrderOptions : EndpointOptions<PlaceFuturesTriggerOrderRequest>
    {
        /// <summary>
        /// When true the API holds the funds until the order is triggered or canceled. When true the funds will only be required when the order is triggered and will fail if the funds are not available at that time.
        /// </summary>
        public bool HoldsFunds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceFuturesTriggerOrderOptions(bool holdsFunds) : base(true)
        {
            HoldsFunds = holdsFunds;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public Error? ValidateRequest(
            string exchange,
            PlaceFuturesTriggerOrderRequest request,
            TradingMode? tradingMode,
            TradingMode[] supportedApiTypes,
            SharedOrderSide side,
            SharedQuantitySupport quantitySupport)
        {
            var quantityError = quantitySupport.Validate(side, request.OrderPrice == null ? SharedOrderType.Market : SharedOrderType.Limit, request.Quantity);
            if (quantityError != null)
                return quantityError;

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }
    }
}
