using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using CryptoExchange.Net.TradeSessions.Interfaces.Handlers;
using System;

namespace CryptoExchange.Net.TradeSessions.Features;

internal static class KlineTrackingFeature
{
    internal static Func<TSession, ITradeSessionFeature> Bind<TSession>(
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

    internal static Func<TSession, ITradeSessionFeature> BindAsync<TSession>(
        ISharedUserClientProvider provider,
        SharedSymbol symbol,
        TradeEnvironment? environment,
        SharedKlineInterval interval,
        int? maxQueuedItems,
        QueueFullBehavior? fullBehavior)
        where TSession : TradeSession, IAsyncKlineUpdateHandler
    {
        var api = SymbolTrackingCapabilities.Resolve(
            provider, symbol, environment, SharedCapabilities.Klines.SubscribeKlines, "kline subscriptions");

        return session => new QueuedSubscriptionFeature<DataEvent<SharedKline>>(
            session,
            (handler, ct) => api.SubscribeToKlineUpdatesAsync(
                new SubscribeKlineRequest(symbol, interval), handler, ct),
            update => session.HandleKlineUpdateAsync(symbol, update),
            maxQueuedItems,
            fullBehavior);
    }
}
