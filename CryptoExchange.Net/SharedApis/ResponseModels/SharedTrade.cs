using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Public trade info
    /// </summary>
    [DebuggerDisplay("[{Timestamp}] {Symbol,nq} {Side.ToString(),nq} {Quantity} @ {Price}")]
    public record SharedTrade : SharedSymbolModel
    {
        /// <summary>
        /// Quantity of the trade
        /// </summary>
        [Obsolete("Use `Quantities` instead")]
        public decimal Quantity => Quantities.QuantityInBaseAsset ?? Quantities.QuantityInContracts ?? 0;
        /// <summary>
        /// The quantities of the trade
        /// </summary>
        public SharedOrderQuantity Quantities { get; set; }
        /// <summary>
        /// Price of the trade
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Trade time
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Trade side. Buy means that the taker took an ask order of the order book, sell means the taker took a bid order of the order book.
        /// </summary>
        public SharedOrderSide? Side { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedTrade(SharedSymbol? sharedSymbol, string symbol, SharedOrderQuantity quantities, decimal price, DateTime timestamp) : base(sharedSymbol, symbol)
        {
            Quantities = quantities;
            Price = price;
            Timestamp = timestamp;
        }
    }
}
