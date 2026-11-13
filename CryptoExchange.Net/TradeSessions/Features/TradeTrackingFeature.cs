using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using CryptoExchange.Net.TradeSessions.Interfaces.Handlers;
using System;

namespace CryptoExchange.Net.TradeSessions.Features;

internal static class TradeTrackingFeature
{
    internal static Func<TSession, ITradeSessionFeature> Bind<TSession>(
        ISharedUserClientProvider provider,
        SharedSymbol symbol,
        TradeEnvironment? environment)
        where TSession : TradeSession, ITradeUpdateHandler
    {
        var api = SymbolTrackingCapabilities.Resolve(
            provider, symbol, environment, SharedCapabilities.Trades.SubscribeTrades, "trade subscriptions");

        return session => new SubscriptionFeature(ct =>
            api.SubscribeToTradeUpdatesAsync(
                new SubscribeTradeRequest(symbol),
                update => session.Dispatch(() => session.HandleTradeUpdate(symbol, update)),
                ct));
    }

    internal static Func<TSession, ITradeSessionFeature> BindAsync<TSession>(
        ISharedUserClientProvider provider,
        SharedSymbol symbol,
        TradeEnvironment? environment,
        int? maxQueuedItems,
        QueueFullBehavior? fullBehavior)
        where TSession : TradeSession, IAsyncTradeUpdateHandler
    {
        var api = SymbolTrackingCapabilities.Resolve(
            provider, symbol, environment, SharedCapabilities.Trades.SubscribeTrades, "trade subscriptions");

        return session => new QueuedSubscriptionFeature<DataEvent<SharedTrade[]>>(
            session,
            (handler, ct) => api.SubscribeToTradeUpdatesAsync(
                new SubscribeTradeRequest(symbol), handler, ct),
            update => session.HandleTradeUpdateAsync(symbol, update),
            maxQueuedItems,
            fullBehavior);
    }
}
