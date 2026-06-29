using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to order updates
    /// </summary>
    public class SubscribeSpotOrderOptions : EndpointOptions<SubscribeSpotOrderRequest, ISpotOrderSocketClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeSpotOrderOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISpotOrderSocketClient.SubscribeToSpotOrderUpdatesAsync))
        {
        }
    }
}
