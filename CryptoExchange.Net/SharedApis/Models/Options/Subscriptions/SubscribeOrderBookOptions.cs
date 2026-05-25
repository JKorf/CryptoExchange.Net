using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to order book snapshot updates
    /// </summary>
    public class SubscribeOrderBookOptions : EndpointOptions<SubscribeOrderBookRequest, IOrderBookSocketClient>
    {
        /// <summary>
        /// Order book depths supported for updates
        /// </summary>
        public int[] SupportedLimits { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeOrderBookOptions(string exchange, bool needsAuthentication, int[] limits) : base(exchange, needsAuthentication)
        {
            SupportedLimits = limits;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(SubscribeOrderBookRequest request, IOrderBookSocketClient client)
        {
            if (request.Limit != null && !SupportedLimits.Contains(request.Limit.Value))
                return ArgumentError.Invalid(nameof(SubscribeOrderBookRequest.Limit), "Limit not supported");

            return base.ValidateRequest(request, client);
        }
    }
}
