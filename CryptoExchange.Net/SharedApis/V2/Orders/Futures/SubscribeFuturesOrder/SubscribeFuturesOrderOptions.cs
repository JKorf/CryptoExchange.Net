using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to order updates
    /// </summary>
    public class SubscribeFuturesOrderOptions : CapabilityOptions<SubscribeFuturesOrderRequest, ISubscribeFuturesOrdersSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to futures order updates";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeFuturesOrderRequest>.Optional(x => x.TradingMode, "Filter futures order updates by trading mode", TradingMode.PerpetualLinear),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeFuturesOrderOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribeFuturesOrdersSocket.SubscribeToFuturesOrderUpdatesAsync), _defaultParameterRules)
        {
        }
    }
}
