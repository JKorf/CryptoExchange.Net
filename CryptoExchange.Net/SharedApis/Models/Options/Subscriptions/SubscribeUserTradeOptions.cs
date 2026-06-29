using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to user trade updates
    /// </summary>
    public class SubscribeUserTradeOptions : EndpointOptions<SubscribeUserTradeRequest, IUserTradeSocketClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeUserTradeOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(IUserTradeSocketClient.SubscribeToUserTradeUpdatesAsync))
        {
        }
    }
}
