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

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeBookTickerRequest>.Optional(x => x.Symbol, "The symbol to subscribe to", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<SubscribeBookTickerRequest>.Optional(x => x.Symbols, "The symbols to subscribe to", new[] { new SharedSymbol(TradingMode.Spot, "ETH", "USDT") }),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeBookTickerOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(ISubscribeBookTickerSocket.SubscribeToBookTickerUpdatesAsync), _defaultParameterRules)
        {
        }
    }
}
