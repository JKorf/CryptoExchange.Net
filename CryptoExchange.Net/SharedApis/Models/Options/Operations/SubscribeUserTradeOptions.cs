using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to user trade updates
    /// </summary>
    public class SubscribeUserTradeOptions : CapabilityOptions<SubscribeUserTradeRequest, ISubscribeUserTradesOperation>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to user trade updates";

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeUserTradeOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribeUserTradesOperation.SubscribeToUserTradeUpdatesAsync))
        {
        }
    }
}
