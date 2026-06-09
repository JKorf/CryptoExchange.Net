using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to position updates
    /// </summary>
    public class SubscribePositionOptions : EndpointOptions<SubscribePositionRequest, IPositionSocketClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SubscribePositionOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(IPositionSocketClient.SubscribeToPositionUpdatesAsync))
        {
        }
    }
}
