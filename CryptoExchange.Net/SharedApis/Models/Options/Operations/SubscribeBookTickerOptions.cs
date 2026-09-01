using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to book ticker updates
    /// </summary>
    public class SubscribeBookTickerOptions : CapabilityOptions<SubscribeBookTickerRequest, ISubscribeBookTickerSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to best bid and ask price updates";

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeBookTickerOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribeBookTickerSocket.SubscribeToBookTickerUpdatesAsync))
        {
        }
    }
}
