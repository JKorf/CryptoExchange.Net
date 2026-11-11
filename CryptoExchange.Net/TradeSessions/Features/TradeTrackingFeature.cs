using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;

namespace CryptoExchange.Net.TradeSessions.Features;

internal static class TradeTrackingFeature
{
    internal static Func<TSession, ITradingSessionFeature> Bind<TSession>(
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
}
