using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to mark price updates
    /// </summary>
    public class SubscribeMarkPriceOptions : CapabilityOptions<SubscribeMarkPriceRequest, ISubscribeMarkPriceSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to mark price updates for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeMarkPriceRequest>.Optional(x => x.Symbol, "The symbol to subscribe to", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<SubscribeMarkPriceRequest>.Optional(x => x.Symbols, "The symbols to subscribe to", new[] { new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT") }),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeMarkPriceOptions(string exchange, bool needsAuthentication)
            : base(exchange, needsAuthentication, nameof(ISubscribeMarkPriceSocket.SubscribeToMarkPriceUpdatesAsync), _defaultParameterRules, SharedTradingModeSets.Futures)
        {
        }
    }
}
