using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Mark price
    /// </summary>
    [DebuggerDisplay("{Symbol,nq}: {MarkPrice}")]
    public record SharedMarkPrice : SharedSymbolModel
    {
        /// <summary>
        /// Current mark price
        /// </summary>
        public decimal MarkPrice { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedMarkPrice(SharedSymbol? sharedSymbol, string symbol, decimal markPrice)
            : base(sharedSymbol, symbol)
        {
            MarkPrice = markPrice;
        }
    }
}
