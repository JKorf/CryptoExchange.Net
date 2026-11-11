using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Sessions;

public class TradingContext
{
    private readonly Func<IReadOnlyCollection<ISymbolOrderBook>> _getOrderBooks;
    private readonly Func<SharedSymbol, ISymbolOrderBook> _getOrderBook;

    public string Exchange { get; }

    public IReadOnlyCollection<ISymbolOrderBook> OrderBooks => _getOrderBooks();

    internal TradingContext(
        string exchange,
        Func<IReadOnlyCollection<ISymbolOrderBook>> getOrderBooks,
        Func<SharedSymbol, ISymbolOrderBook> getOrderBook)
    {
        Exchange = exchange;
        _getOrderBooks = getOrderBooks;
        _getOrderBook = getOrderBook;
    }

    public ISymbolOrderBook GetOrderBook(SharedSymbol symbol)
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));

        return _getOrderBook(symbol);
    }
}

public sealed class TradingContext<TUser> : TradingContext
    where TUser : class, ITradingUser
{
    public TradingSessionUsers<TUser> User { get; }

    internal TradingContext(
        string exchange,
        TradingSessionUsers<TUser> users,
        Func<IReadOnlyCollection<ISymbolOrderBook>> getOrderBooks,
        Func<SharedSymbol, ISymbolOrderBook> getOrderBook)
        : base(exchange, getOrderBooks, getOrderBook)
    {
        User = users;
    }
}
