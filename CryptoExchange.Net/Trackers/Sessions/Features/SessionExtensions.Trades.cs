using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        public static SymbolTrackingBuilder<TSession> AddTradeTracking<TSession>(
            this SymbolTrackingBuilder<TSession> builder)
            where TSession : Session, ITradeUpdateHandler
            => AddTradeTracking<TSession, SymbolTrackingBuilder<TSession>>(builder);

        public static TBuilder AddTradeTracking<TSession, TBuilder>(
            this SymbolTrackingBuilder<TSession, TBuilder> builder)
            where TSession : Session, ITradeUpdateHandler
            where TBuilder : SymbolTrackingBuilder<TSession, TBuilder>
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
                return session => new SubscriptionFeature(ct =>
                    api.SubscribeToTradeUpdatesAsync(
                        new SubscribeTradeRequest(symbol),
                        update => { if (session.Started) { session.HandleTradeUpdate(symbol, update); } },
                        ct));
            });
        }
    }
}
