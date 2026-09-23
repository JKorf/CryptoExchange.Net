using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Symbol request
    /// </summary>
    public record SharedSymbolRequest : SharedRequest
    {
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
        public SharedSymbolRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol.TradingMode, exchangeParameters)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSymbolRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null) 
            : base(symbols.FirstOrDefault()?.TradingMode ?? throw new ArgumentException("Empty symbol list"), exchangeParameters)
        {
            Symbols = symbols.ToArray();

            if (symbols.GroupBy(x => x.TradingMode).Count() > 1)
                throw new ArgumentException("All symbols in the symbol list should have the same trading mode");
        }

        /// <summary>
        /// Get the symbol name using the provided formatter
        /// </summary>
        public string SymbolName(Func<string, string, TradingMode, DateTime?, string> formatter)
            => Symbol?.GetSymbol(formatter) ?? throw new ArgumentException("Symbol is not set");

        /// <summary>
        /// Get the symbol names using the provided formatter
        /// </summary>
        public string[] SymbolNames(Func<string, string, TradingMode, DateTime?, string> formatter)
            => Symbols?.Select(x => x.GetSymbol(formatter)).ToArray() ?? (Symbol != null ? new[] { Symbol.GetSymbol(formatter) } : null) ?? throw new ArgumentException("Symbol is not set");
    }
}
