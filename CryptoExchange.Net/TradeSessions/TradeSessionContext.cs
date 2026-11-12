using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Trade session context
/// </summary>
public class TradeSessionContext
{
    /// <summary>
    /// Exchange name
    /// </summary>
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

/// <inheritdoc />
public sealed class TradeSessionContext<TUser> : TradeSessionContext
    where TUser : class, ITradeSessionUser
{
    /// <summary>
    /// User access
    /// </summary>
    public TradeSessionUsers<TUser> User { get; }

    internal TradeSessionContext(
        string exchange,
        TradeSessionUsers<TUser> users,
        Func<IReadOnlyDictionary<SharedSymbol, ISymbolOrderBook>> getOrderBooks,
        Func<SharedSymbol, ISymbolOrderBook?> getOrderBook)
        : base(exchange, getOrderBooks, getOrderBook)
    {
        User = users;
    }
}
