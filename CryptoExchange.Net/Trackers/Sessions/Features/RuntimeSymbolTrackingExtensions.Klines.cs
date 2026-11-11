using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live kline updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TradingSessionSymbolBuilder<TStrategy> AddKlineUpdates<TStrategy>(
            this TradingSessionSymbolBuilder<TStrategy> builder, SharedKlineInterval interval)
            where TStrategy : TradingStrategy, IKlineUpdateHandler
            => AddKlineUpdates<TStrategy, TradingSessionSymbolBuilder<TStrategy>>(
                builder, interval);

        /// <summary>
        /// Add live kline updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <typeparam name="TBuilder">The builder</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TBuilder AddKlineUpdates<TStrategy, TBuilder>(
            this TradingSessionSymbolBuilder<TStrategy, TBuilder> builder, SharedKlineInterval interval)
            where TStrategy : TradingStrategy, IKlineUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>
        {
            var symbol = builder.Symbol;
            var environment = builder.Environment;
            return builder.AddFeature("klines", clientProvider =>
            {
                var client = clientProvider.GetPublicSharedApiClient(environment);
                var resolved = client.GetCapability(
                    SharedCapabilities.Klines.SubscribeKlines, symbol.TradingMode)
                    ?? throw MissingCapability(client, symbol, "kline subscriptions");

                var api = resolved.Capability;
                return strategy => new SubscriptionFeature(ct =>
                    api.SubscribeToKlineUpdatesAsync(
                        new SubscribeKlineRequest(symbol, interval),
                        update => strategy.Dispatch(() => strategy.HandleKlineUpdate(symbol, update)),
                        ct));
            });
        }
    }
}
