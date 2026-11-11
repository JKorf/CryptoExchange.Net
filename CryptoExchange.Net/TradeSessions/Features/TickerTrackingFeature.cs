using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;

namespace CryptoExchange.Net.TradeSessions.Features;

internal static class TickerTrackingFeature
{
    internal static Func<TSession, ITradingSessionFeature> Bind<TSession>(
        ISharedUserClientProvider provider,
        SharedSymbol symbol,
        TradeEnvironment? environment)
        where TSession : TradeSession, ITickerUpdateHandler
    {
        var api = SymbolTrackingCapabilities.Resolve(
            provider, symbol, environment, SharedCapabilities.Tickers.SubscribeTicker, "ticker subscriptions");

        return session => new SubscriptionFeature(ct =>
            api.SubscribeToTickerUpdatesAsync(
                new SubscribeTickerRequest(symbol),
                update => session.Dispatch(() => session.HandleTickerUpdate(symbol, update)),
                ct));
    }
}
