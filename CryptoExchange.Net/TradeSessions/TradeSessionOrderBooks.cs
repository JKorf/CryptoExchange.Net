using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Read-only access to the active order books managed by a trade session
/// </summary>
public sealed class TradeSessionOrderBooks : IReadOnlyDictionary<SharedSymbol, ISymbolOrderBook>
{
    private readonly Func<IReadOnlyDictionary<SharedSymbol, ISymbolOrderBook>> _getSnapshot;
    private readonly Func<SharedSymbol, ISymbolOrderBook?> _getOrderBook;

    /// <inheritdoc />
    public int Count => _getSnapshot().Count;

    /// <inheritdoc />
    public IEnumerable<SharedSymbol> Keys => _getSnapshot().Keys;

    /// <inheritdoc />
    public IEnumerable<ISymbolOrderBook> Values => _getSnapshot().Values;

    /// <summary>
    /// Get the active order book for a symbol
    /// </summary>
    /// <exception cref="KeyNotFoundException">No active order book exists for the symbol</exception>
    public ISymbolOrderBook this[SharedSymbol symbol]
        => TryGetValue(symbol, out var book)
            ? book
            : throw new KeyNotFoundException($"No active order book exists for {symbol}.");

    internal TradeSessionOrderBooks(
        Func<IReadOnlyDictionary<SharedSymbol, ISymbolOrderBook>> getSnapshot,
        Func<SharedSymbol, ISymbolOrderBook?> getOrderBook)
    {
        _getSnapshot = getSnapshot;
        _getOrderBook = getOrderBook;
    }

    /// <inheritdoc />
    public bool ContainsKey(SharedSymbol symbol) => TryGetValue(symbol, out _);

    /// <inheritdoc />
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public bool TryGetValue(SharedSymbol symbol, [MaybeNullWhen(false)] out ISymbolOrderBook book)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));

        book = _getOrderBook(symbol);
        return book != null;
    }

    /// <summary>
    /// Enumerate a snapshot of the active order books
    /// </summary>
    public IEnumerator<KeyValuePair<SharedSymbol, ISymbolOrderBook>> GetEnumerator()
        => _getSnapshot().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
