using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to funding info updates
    /// </summary>
    public class SubscribeFundingInfoOptions : CapabilityOptions<SubscribeFundingInfoRequest, ISubscribeFundingInfoSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to funding info updates for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeFundingInfoRequest>.Optional(x => x.Symbol, "The symbol to subscribe to", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<SubscribeFundingInfoRequest>.Optional(x => x.Symbols, "The symbols to subscribe to", new[] { new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT") }),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeFundingInfoOptions(string exchange, bool needsAuthentication)
            : base(exchange, needsAuthentication, nameof(ISubscribeFundingInfoSocket.SubscribeToFundingInfoUpdatesAsync), _defaultParameterRules, SharedTradingModeSets.Futures)
        {
        }
    }
}
