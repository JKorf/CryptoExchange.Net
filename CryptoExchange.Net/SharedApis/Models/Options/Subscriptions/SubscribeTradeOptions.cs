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
        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTradeOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ITradeSocketClient.SubscribeToTradeUpdatesAsync))
        {
        }
    }
}
