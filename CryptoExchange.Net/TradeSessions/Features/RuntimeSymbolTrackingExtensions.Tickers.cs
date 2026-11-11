using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;

namespace CryptoExchange.Net.TradeSessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/>
        /// to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradeSessionSymbolBuilder<TStrategy> AddTickerUpdates<TStrategy>(
            this TradeSessionSymbolBuilder<TStrategy> builder)
            where TStrategy : TradeSession, ITickerUpdateHandler
            => AddTickerUpdates<TStrategy, TradeSessionSymbolBuilder<TStrategy>>(
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
            where TStrategy : TradeSession, ITickerUpdateHandler
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
