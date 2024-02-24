using System;

namespace CryptoExchange.Net.Trackers.Klines
{
    /// <summary>
    /// A kline for tracking purposes
    /// </summary>
    public interface IKlineItem
    {
        /// <summary>
        /// The open time of the kline
        /// </summary>
        DateTime OpenTime { get; }
        /// <summary>
        /// Open price
        /// </summary>
        decimal OpenPrice { get; }
        /// <summary>
        /// High price
        /// </summary>
        decimal HighPrice { get; }
        /// <summary>
        /// Low price
        /// </summary>
        decimal LowPrice { get; }
        /// <summary>
        /// Close price
        /// </summary>
        decimal ClosePrice { get; }
        /// <summary>
        /// Volume
        /// </summary>
        decimal Volume { get; }
    }
}
