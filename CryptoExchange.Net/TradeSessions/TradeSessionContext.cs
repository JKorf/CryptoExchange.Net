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
    private readonly Func<DateTime> _getUtcNow;

    /// <summary>
    /// Current UTC time of the session's execution environment.
    /// </summary>
    public DateTime UtcNow => _getUtcNow();

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
        Func<SharedSymbol, ISymbolOrderBook?> getOrderBook,
        Func<DateTime>? getUtcNow = null)
    {
        Exchange = exchange;
        _getUtcNow = getUtcNow ?? (() => DateTime.UtcNow);
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
        Func<SharedSymbol, ISymbolOrderBook?> getOrderBook,
        Func<DateTime>? getUtcNow = null)
        : base(exchange, getOrderBooks, getOrderBook, getUtcNow)
    {
        User = users;
    }
}
