using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        public static SymbolTrackingBuilder<TSession> AddKlineTracking<TSession>(
            this SymbolTrackingBuilder<TSession> builder,
            SharedKlineInterval interval)
            where TSession : Session, IKlineUpdateHandler
            => AddKlineTracking<TSession, SymbolTrackingBuilder<TSession>>(builder, interval);

        public static TBuilder AddKlineTracking<TSession, TBuilder>(
            this SymbolTrackingBuilder<TSession, TBuilder> builder, SharedKlineInterval interval)
            where TSession : Session, IKlineUpdateHandler
            where TBuilder : SymbolTrackingBuilder<TSession, TBuilder>
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
                return session => new SubscriptionFeature(ct =>
                    api.SubscribeToKlineUpdatesAsync(
                        new SubscribeKlineRequest(symbol, interval),
                        update => { if (session.Started) { session.HandleKlineUpdate(symbol, update); } },
                        ct));
            });
        }
    }
}
