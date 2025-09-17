using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Klines;
using CryptoExchange.Net.Trackers.Trades;
using System;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Tracker factory
    /// </summary>
    public interface ITrackerFactory
    {
        /// <summary>
        /// Whether the factory supports creating a KlineTracker instance for this symbol and interval
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="interval">The kline interval</param>
        bool CanCreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval);

        /// <summary>
        /// Create a new kline tracker
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="interval">Kline interval</param>
        /// <param name="limit">The max amount of klines to retain</param>
        /// <param name="period">The max period the data should be retained</param>
        /// <returns></returns>
        IKlineTracker CreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval, int? limit = null, TimeSpan? period = null);

        /// <summary>
        /// Whether the factory supports creating a TradeTracker instance for this symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        bool CanCreateTradeTracker(SharedSymbol symbol);

        /// <summary>
        /// Create a new trade tracker for a symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="limit">The max amount of trades to retain</param>
        /// <param name="period">The max period the data should be retained</param>
        /// <returns></returns>
        ITradeTracker CreateTradeTracker(SharedSymbol symbol, int? limit = null, TimeSpan? period = null);
    }
}
