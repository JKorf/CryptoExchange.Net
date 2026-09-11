using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to trade updates
    /// </summary>
    public class SubscribeTradeOptions : CapabilityOptions<SubscribeTradeRequest, ISubscribeTradesSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to public trade updates for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeTradeRequest>.Optional(x => x.Symbol, "The symbol to subscribe to", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<SubscribeTradeRequest>.Optional(x => x.Symbols, "The symbols to subscribe to", new[] { new SharedSymbol(TradingMode.Spot, "ETH", "USDT") }),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTradeOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribeTradesSocket.SubscribeToTradeUpdatesAsync), _defaultParameterRules)
        {
        }
    }
}
