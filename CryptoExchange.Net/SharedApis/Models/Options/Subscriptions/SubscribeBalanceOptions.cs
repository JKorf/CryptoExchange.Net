using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to balance updates
    /// </summary>
    public class SubscribeBalanceOptions : EndpointOptions<SubscribeBalancesRequest, IBalanceSocketClient>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to balance updates";

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeBalanceOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(IBalanceSocketClient.SubscribeToBalanceUpdatesAsync))
        {
        }
    }
}
