using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.TradeSessions;

public class TradeSessionContext
{
    public string Exchange { get; }

    /// <summary>
    /// The active order books managed by the session
    /// </summary>
    public TradeSessionOrderBooks OrderBooks { get; }

    internal TradeSessionContext(
        string exchange,
        Func<IReadOnlyDictionary<SharedSymbol, ISymbolOrderBook>> getOrderBooks,
        Func<SharedSymbol, ISymbolOrderBook?> getOrderBook)
    {
        Exchange = exchange;
        OrderBooks = new TradeSessionOrderBooks(getOrderBooks, getOrderBook);
    }
}

public sealed class TradingContext<TUser> : TradeSessionContext
    where TUser : class, ITradingUser
{
    public TradingSessionUsers<TUser> User { get; }

    internal TradingContext(
        string exchange,
        TradingSessionUsers<TUser> users,
        Func<IReadOnlyDictionary<SharedSymbol, ISymbolOrderBook>> getOrderBooks,
        Func<SharedSymbol, ISymbolOrderBook?> getOrderBook)
        : base(exchange, getOrderBooks, getOrderBook)
    {
        User = users;
    }
}
