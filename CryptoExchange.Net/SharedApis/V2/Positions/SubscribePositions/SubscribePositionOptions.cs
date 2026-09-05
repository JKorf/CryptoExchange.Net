using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to position updates
    /// </summary>
    public class SubscribePositionOptions : CapabilityOptions<SubscribePositionRequest, ISubscribePositionsSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to futures position updates";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribePositionRequest>.Optional(x => x.TradingMode, "Filter position updates by trading mode", TradingMode.PerpetualLinear),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribePositionOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribePositionsSocket.SubscribeToPositionUpdatesAsync), _defaultParameterRules)
        {
        }
    }
}
