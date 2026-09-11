using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to balance updates
    /// </summary>
    public class SubscribeBalanceOptions : CapabilityOptions<SubscribeBalancesRequest, ISubscribeBalancesSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to balance updates";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeBalancesRequest>.Optional(x => x.TradingMode, "Filter balance updates by trading mode", TradingMode.Spot),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeBalanceOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribeBalancesSocket.SubscribeToBalanceUpdatesAsync), _defaultParameterRules)
        {
        }
    }
}
