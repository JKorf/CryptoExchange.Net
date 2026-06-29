using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to book ticker updates
    /// </summary>
    public class SubscribeBookTickerOptions : EndpointOptions<SubscribeBookTickerRequest, IBookTickerSocketClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeBookTickerOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(IBookTickerSocketClient.SubscribeToBookTickerUpdatesAsync))
        {
        }
    }
}
