using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to order updates
    /// </summary>
    public class SubscribeFuturesOrderOptions : EndpointOptions<SubscribeFuturesOrderRequest, IFuturesOrderSocketClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeFuturesOrderOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(IFuturesOrderSocketClient.SubscribeToFuturesOrderUpdatesAsync))
        {
        }
    }
}
