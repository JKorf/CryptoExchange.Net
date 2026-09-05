using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to order book snapshot updates
    /// </summary>
    public class SubscribeIncrementalOrderBookOptions : CapabilityOptions<SubscribeOrderBookRequest, ISubscribeIncrementalOrderBookSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to incremental order book updates for a symbol";

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
        /// The type of updates the subscription produces
        /// </summary>
        public SharedOrderBookSubscriptionType UpdateType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeIncrementalOrderBookOptions(string exchange, bool needsAuthentication, int[] limits, SharedOrderBookSubscriptionType updateType)
            : base(exchange, needsAuthentication, nameof(ISubscribeIncrementalOrderBookSocket.SubscribeToOrderBookUpdatesAsync), _defaultParameterRules)
        {
            SupportedLimits = limits;
            UpdateType = updateType;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(SubscribeOrderBookRequest request, ISubscribeIncrementalOrderBookSocket client)
        {
            if (request.Limit != null && !SupportedLimits.Contains(request.Limit.Value))
                return ArgumentError.Invalid(nameof(SubscribeOrderBookRequest.Limit), "Limit not supported");

            return base.ValidateRequest(request, client);
        }
    }
}
