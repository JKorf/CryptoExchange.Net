using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live kline updates to <typeparamref name="TSession"/>. Requires <typeparamref name="TSession"/> to implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TradingSessionSymbolBuilder<TSession> AddKlineUpdates<TSession>(
            this TradingSessionSymbolBuilder<TSession> builder,
            SharedKlineInterval interval)
            where TSession : TradingSession, IKlineUpdateHandler
            => AddKlineUpdates<TSession, TradingSessionSymbolBuilder<TSession>>(builder, interval);

        /// <summary>
        /// Add live kline updates to <typeparamref name="TSession"/>. Requires <typeparamref name="TSession"/> to implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TSession">The implementation type.</typeparam>
        /// <typeparam name="TBuilder">The builder</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TBuilder AddKlineUpdates<TSession, TBuilder>(
            this TradingSessionSymbolBuilder<TSession, TBuilder> builder, SharedKlineInterval interval)
            where TSession : TradingSession, IKlineUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TSession, TBuilder>
        {
            var symbol = builder.Symbol;
            var environment = builder.Environment;
            return builder.AddFeature("klines", clientProvider =>
            {
                var client = clientProvider.GetPublicSharedApiClient(environment);
                var resolved = client.GetCapability(SharedCapabilities.Klines.SubscribeKlines, symbol.TradingMode)
                    ?? throw MissingCapability(client, symbol, "kline subscriptions");

                var api = resolved.Capability;
                return session => new SubscriptionFeature(ct =>
                    api.SubscribeToKlineUpdatesAsync(new SubscribeKlineRequest(symbol, interval),
                        update =>
                        { 
                            if (session.Started) 
                                session.HandleKlineUpdate(symbol, update);
                        },
                        ct));
            });
        }
    }
}
