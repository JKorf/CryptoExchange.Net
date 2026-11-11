using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live trade updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradingSessionSymbolBuilder<TStrategy> AddTradeUpdates<TStrategy>(
            this TradingSessionSymbolBuilder<TStrategy> builder)
            where TStrategy : TradingStrategy, ITradeUpdateHandler
            => AddTradeUpdates<TStrategy, TradingSessionSymbolBuilder<TStrategy>>(
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
            where TStrategy : TradingStrategy, ITradeUpdateHandler
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
