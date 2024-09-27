using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Public trade info
    /// </summary>
    public record SharedTrade
    {
        /// <summary>
        /// Quantity of the trade
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Price of the trade
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Trade time
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedTrade(decimal quantity, decimal price, DateTime timestamp)
        {
            Quantity = quantity;
            Price = price;
            Timestamp = timestamp;
        }
    }
}
