using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Symbol request
/// </summary>
public record SharedSymbolRequest : SharedRequest
{
    /// <summary>
    /// Trading mode
    /// </summary>
    public TradingMode TradingMode { get; }
    /// <summary>
    /// The symbol
    /// </summary>
    public SharedSymbol? Symbol { get; set; }
    /// <summary>
    /// Symbols
    /// </summary>
    public SharedSymbol[]? Symbols { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public SharedSymbolRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
    {
        Symbol = symbol;
        TradingMode = symbol.TradingMode;
    }

    /// <summary>
    /// ctor
    /// </summary>
    public SharedSymbolRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
    {
        if (!symbols.Any())
            throw new ArgumentException("Empty symbol list");

        if (symbols.GroupBy(x => x.TradingMode).Count() > 1)
            throw new ArgumentException("All symbols in the symbol list should have the same trading mode");

        Symbols = symbols.ToArray();
        TradingMode = Symbols.First().TradingMode;
    }
}
