using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to Index price updates
    /// </summary>
    public class SubscribeIndexPriceOptions : CapabilityOptions<SubscribeIndexPriceRequest, ISubscribeIndexPriceSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to Index price updates for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeIndexPriceRequest>.Optional(x => x.Symbol, "The symbol to subscribe to", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<SubscribeIndexPriceRequest>.Optional(x => x.Symbols, "The symbols to subscribe to", new[] { new SharedSymbol(TradingMode.Spot, "ETH", "USDT") }),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeIndexPriceOptions(string exchange, bool needsAuthentication)
            : base(exchange, needsAuthentication, nameof(ISubscribeIndexPriceSocket.SubscribeToIndexPriceUpdatesAsync), _defaultParameterRules)
        {
        }
    }
}
