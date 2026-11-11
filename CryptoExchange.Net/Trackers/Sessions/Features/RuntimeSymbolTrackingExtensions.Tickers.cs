using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TSession"/>. Requires <typeparamref name="TSession"/> to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradingSessionSymbolBuilder<TSession> AddTickerUpdates<TSession>(
            this TradingSessionSymbolBuilder<TSession> builder)
            where TSession : TradingSession, ITickerUpdateHandler
            => AddTickerUpdates<TSession, TradingSessionSymbolBuilder<TSession>>(builder);

        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TSession"/>. Requires <typeparamref name="TSession"/> to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder.</typeparam>
        /// <typeparam name="TSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTickerUpdates<TSession, TBuilder>(
            this TradingSessionSymbolBuilder<TSession, TBuilder> builder)
            where TSession : TradingSession, ITickerUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TSession, TBuilder>
        {
            var symbol = builder.Symbol;
            var environment = builder.Environment;
            return builder.AddFeature("tickers", clientProvider =>
            {
                var client = clientProvider.GetPublicSharedApiClient(environment);
                var resolved = client.GetCapability(SharedCapabilities.Tickers.SubscribeTicker, symbol.TradingMode)
                    ?? throw MissingCapability(client, symbol, "ticker subscriptions");

                var api = resolved.Capability;
                return session => new SubscriptionFeature(ct =>
                    api.SubscribeToTickerUpdatesAsync(
                        new SubscribeTickerRequest(symbol),
                        update =>
                        {
                            if (session.Started) 
                                session.HandleTickerUpdate(symbol, update);
                        },
                        ct));
            });
        }
    }
}
