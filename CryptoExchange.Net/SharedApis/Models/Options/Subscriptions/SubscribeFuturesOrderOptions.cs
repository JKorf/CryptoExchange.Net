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
        /// <inheritdoc />
        public override string Description => "Subscribe to futures order updates";

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeFuturesOrderOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(IFuturesOrderSocketClient.SubscribeToFuturesOrderUpdatesAsync))
        {
        }
    }
}
