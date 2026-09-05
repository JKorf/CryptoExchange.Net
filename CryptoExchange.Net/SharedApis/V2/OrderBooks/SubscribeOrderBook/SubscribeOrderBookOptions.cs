using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to order book snapshot updates
    /// </summary>
    public class SubscribeOrderBookOptions : CapabilityOptions<SubscribeOrderBookRequest, ISubscribeOrderBookSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to order book updates for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeOrderBookRequest>.Optional(x => x.Symbol, "The symbol to subscribe to", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<SubscribeOrderBookRequest>.Optional(x => x.Symbols, "The symbols to subscribe to", new[] { new SharedSymbol(TradingMode.Spot, "ETH", "USDT") }),
            RequestParameterRule<SubscribeOrderBookRequest>.Optional(x => x.Limit, "The order book depth", 100),
        };

        /// <summary>
        /// Order book depths supported for updates
        /// </summary>
        public int[] SupportedLimits { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeOrderBookOptions(string exchange, bool needsAuthentication, int[] limits) : base(exchange, needsAuthentication, nameof(ISubscribeOrderBookSocket.SubscribeToOrderBookUpdatesAsync), _defaultParameterRules)
        {
            SupportedLimits = limits;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(SubscribeOrderBookRequest request, ISubscribeOrderBookSocket client)
        {
            if (request.Limit != null && !SupportedLimits.Contains(request.Limit.Value))
                return ArgumentError.Invalid(nameof(SubscribeOrderBookRequest.Limit), "Limit not supported");

            return base.ValidateRequest(request, client);
        }
    }
}
