using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Index price
    /// </summary>
    [DebuggerDisplay("{Symbol,nq}: {IndexPrice}")]
    public record SharedIndexPrice : SharedSymbolModel
    {
        /// <summary>
        /// Current index price
        /// </summary>
        public decimal IndexPrice { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedIndexPrice(SharedSymbol? sharedSymbol, string symbol, decimal indexPrice)
            : base(sharedSymbol, symbol)
        {
            IndexPrice = indexPrice;
        }
    }
}
