using System;

namespace CryptoExchange.Net.Trackers.Trades
{
    /// <summary>
    /// A trade for tracking purposes
    /// </summary>
    public interface ITradeItem
    {
        /// <summary>
        /// Id of the trade
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Quantity of the trade
        /// </summary>
        decimal Quantity { get; }
        /// <summary>
        /// Price of the trade
        /// </summary>
        decimal Price { get; }
        /// <summary>
        /// Timestamp the trade happened
        /// </summary>
        DateTime Timestamp { get; }
    }
}
