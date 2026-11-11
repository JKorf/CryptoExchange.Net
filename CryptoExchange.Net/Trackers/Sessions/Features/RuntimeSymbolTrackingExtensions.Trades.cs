using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        /// <summary>
        /// Add live trade updates to <typeparamref name="TSession"/>. Requires <typeparamref name="TSession"/> to implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradingSessionSymbolBuilder<TSession> AddTradeUpdates<TSession>(
            this TradingSessionSymbolBuilder<TSession> builder)
            where TSession : TradingSession, ITradeUpdateHandler
            => AddTradeUpdates<TSession, TradingSessionSymbolBuilder<TSession>>(builder);


        /// <summary>
        /// Add live trade updates to <typeparamref name="TSession"/>. Requires <typeparamref name="TSession"/> to implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder type.</typeparam>
        /// <typeparam name="TSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTradeUpdates<TSession, TBuilder>(
            this TradingSessionSymbolBuilder<TSession, TBuilder> builder)
            where TSession : TradingSession, ITradeUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TSession, TBuilder>
        {
            var symbol = builder.Symbol;
            var environment = builder.Environment;
            return builder.AddFeature("trades", clientProvider =>
            {
                var client = clientProvider.GetPublicSharedApiClient(environment);
                var resolved = client.GetCapability(SharedCapabilities.Trades.SubscribeTrades, symbol.TradingMode)
                    ?? throw MissingCapability(client, symbol, "trade subscriptions");

                var api = resolved.Capability;
                return session => new SubscriptionFeature(ct =>
                    api.SubscribeToTradeUpdatesAsync(
                        new SubscribeTradeRequest(symbol),
                        update => 
                        { 
                            if (session.Started) 
                                session.HandleTradeUpdate(symbol, update); 
                        },
                        ct));
            });
        }
    }
}
