using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to user trade updates
    /// </summary>
    public class SubscribeUserTradeOptions : CapabilityOptions<SubscribeUserTradeRequest, ISubscribeUserTradesSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to user trade updates";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeUserTradeRequest>.Optional(x => x.TradingMode, "Filter user trade updates by trading mode", TradingMode.Spot),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeUserTradeOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribeUserTradesSocket.SubscribeToUserTradeUpdatesAsync), _defaultParameterRules)
        {
        }
    }
}
