using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public static partial class RuntimeSymbolTrackingExtensions
    {
        public static SymbolTrackingBuilder<TSession> AddTickerTracking<TSession>(
            this SymbolTrackingBuilder<TSession> builder)
            where TSession : Session, ITickerUpdateHandler
            => AddTickerTracking<TSession, SymbolTrackingBuilder<TSession>>(builder);

        public static TBuilder AddTickerTracking<TSession, TBuilder>(
            this SymbolTrackingBuilder<TSession, TBuilder> builder)
            where TSession : Session, ITickerUpdateHandler
            where TBuilder : SymbolTrackingBuilder<TSession, TBuilder>
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
                return session => new SubscriptionFeature(ct =>
                    api.SubscribeToTickerUpdatesAsync(
                        new SubscribeTickerRequest(symbol),
                        update => { if (session.Started) { session.HandleTickerUpdate(symbol, update); } },
                        ct));
            });
        }
    }
}
