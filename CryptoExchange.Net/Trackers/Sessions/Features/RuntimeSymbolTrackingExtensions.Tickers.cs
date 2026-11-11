using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/>
        /// to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradingSessionSymbolBuilder<TStrategy> AddTickerUpdates<TStrategy>(
            this TradingSessionSymbolBuilder<TStrategy> builder)
            where TStrategy : TradingStrategy, ITickerUpdateHandler
            => AddTickerUpdates<TStrategy, TradingSessionSymbolBuilder<TStrategy>>(
                builder);

        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/>
        /// to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder.</typeparam>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTickerUpdates<TStrategy, TBuilder>(
            this TradingSessionSymbolBuilder<TStrategy, TBuilder> builder)
            where TStrategy : TradingStrategy, ITickerUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>, ITickerTrackingBuilder
        {
            var symbol = builder.Symbol;
            var environment = builder.Environment;
            return builder.AddFeature("tickers", clientProvider =>
            {
                var client = clientProvider.GetPublicSharedApiClient(environment);
                var resolved = client.GetCapability(
                    SharedCapabilities.Tickers.SubscribeTicker, symbol.TradingMode)
                    ?? throw MissingCapability(client, symbol, "ticker subscriptions");

                var api = resolved.Capability;
                return strategy => new SubscriptionFeature(ct =>
                    api.SubscribeToTickerUpdatesAsync(
                        new SubscribeTickerRequest(symbol),
                        update => strategy.Dispatch(() => strategy.HandleTickerUpdate(symbol, update)),
                        ct));
            });
        }
    }
}
