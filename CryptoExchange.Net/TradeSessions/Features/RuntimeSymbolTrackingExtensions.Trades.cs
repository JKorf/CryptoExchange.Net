using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;

namespace CryptoExchange.Net.TradeSessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live trade updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradeSessionSymbolBuilder<TStrategy> AddTradeUpdates<TStrategy>(
            this TradeSessionSymbolBuilder<TStrategy> builder)
            where TStrategy : TradeSession, ITradeUpdateHandler
            => AddTradeUpdates<TStrategy, TradeSessionSymbolBuilder<TStrategy>>(
                builder);

        /// <summary>
        /// Add live trade updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder type.</typeparam>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTradeUpdates<TStrategy, TBuilder>(
            this TradingSessionSymbolBuilder<TStrategy, TBuilder> builder)
            where TStrategy : TradeSession, ITradeUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>, ITradeTrackingBuilder
        {
            var symbol = builder.Symbol;
            var environment = builder.Environment;
            return builder.AddFeature("trades", clientProvider =>
            {
                var client = clientProvider.GetPublicSharedApiClient(environment);
                var resolved = client.GetCapability(
                    SharedCapabilities.Trades.SubscribeTrades, symbol.TradingMode)
                    ?? throw MissingCapability(client, symbol, "trade subscriptions");

                var api = resolved.Capability;
                return strategy => new SubscriptionFeature(ct =>
                    api.SubscribeToTradeUpdatesAsync(
                        new SubscribeTradeRequest(symbol),
                        update => strategy.Dispatch(() => strategy.HandleTradeUpdate(symbol, update)),
                        ct));
            });
        }
    }
}
