using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;

namespace CryptoExchange.Net.TradeSessions.Features;

internal static class KlineTrackingFeature
{
    internal static Func<TSession, ITradingSessionFeature> Bind<TSession>(
        ISharedUserClientProvider provider,
        SharedSymbol symbol,
        TradeEnvironment? environment,
        SharedKlineInterval interval)
        where TSession : TradeSession, IKlineUpdateHandler
    {
        var api = SymbolTrackingCapabilities.Resolve(
            provider, symbol, environment, SharedCapabilities.Klines.SubscribeKlines, "kline subscriptions");

        return session => new SubscriptionFeature(ct =>
            api.SubscribeToKlineUpdatesAsync(
                new SubscribeKlineRequest(symbol, interval),
                update => session.Dispatch(() => session.HandleKlineUpdate(symbol, update)),
                ct));
    }
}
