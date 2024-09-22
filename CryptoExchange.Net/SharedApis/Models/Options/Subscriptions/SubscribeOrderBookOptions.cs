using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.Options.Endpoints;
using CryptoExchange.Net.SharedApis.Models.Socket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis.Models.Options.Subscriptions
{
    /// <summary>
    /// Options for subscribing to order book snapshot updates
    /// </summary>
    public class SubscribeOrderBookOptions : EndpointOptions<SubscribeOrderBookRequest>
    {
        /// <summary>
        /// Order book depths supported for updates
        /// </summary>
        public IEnumerable<int> SupportedLimits { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeOrderBookOptions(bool needsAuthentication, IEnumerable<int> limits) : base(needsAuthentication)
        {
            SupportedLimits = limits;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(string exchange, SubscribeOrderBookRequest request, TradingMode? apiType, TradingMode[] supportedApiTypes)
        {
            if (request.Limit != null && !SupportedLimits.Contains(request.Limit.Value))
                return new ArgumentError("Limit not supported");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }
    }
}
