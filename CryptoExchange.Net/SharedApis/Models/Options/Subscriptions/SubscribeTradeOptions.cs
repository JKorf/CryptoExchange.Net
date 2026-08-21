using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to trade updates
    /// </summary>
    public class SubscribeTradeOptions : EndpointOptions<SubscribeTradeRequest, ITradeSocketClient>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to public trade updates for a symbol";

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTradeOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ITradeSocketClient.SubscribeToTradeUpdatesAsync))
        {
        }
    }
}
