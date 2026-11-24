using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using CryptoExchange.Net.TradeSessions.Interfaces.Handlers;
using System;

namespace CryptoExchange.Net.TradeSessions.Features;

internal static class TickerTrackingFeature
{
    internal static Func<TSession, ITradeSessionFeature> Bind<TSession>(
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

    internal static Func<TSession, ITradeSessionFeature> BindAsync<TSession>(
        ISharedUserClientProvider provider,
        SharedSymbol symbol,
        TradeEnvironment? environment,
        int? maxQueuedItems,
        QueueFullBehavior? fullBehavior)
        where TSession : TradeSession, IAsyncTickerUpdateHandler
    {
        var api = SymbolTrackingCapabilities.Resolve(
            provider, symbol, environment, SharedCapabilities.Tickers.SubscribeTicker, "ticker subscriptions");

        return session => new QueuedSubscriptionFeature<DataEvent<SharedTicker>>(
            session,
            (handler, ct) => api.SubscribeToTickerUpdatesAsync(
                new SubscribeTickerRequest(symbol), handler, ct),
            update => session.HandleTickerUpdateAsync(symbol, update),
            maxQueuedItems,
            fullBehavior);
    }
}
