using System;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Common kline
    /// </summary>
    public interface ICommonKline
    {
        /// <summary>
        /// High price for this kline
        /// </summary>
        decimal CommonHigh { get; }
        /// <summary>
        /// Low price for this kline
        /// </summary>
        decimal CommonLow { get; }
        /// <summary>
        /// Open price for this kline
        /// </summary>
        decimal CommonOpen { get; }
        /// <summary>
        /// Close price for this kline
        /// </summary>
        decimal CommonClose { get; }
        /// <summary>
        /// Open time for this kline
        /// </summary>
        DateTime CommonOpenTime { get; }
        /// <summary>
        /// Volume of this kline
        /// </summary>
        decimal CommonVolume { get; }
    }
}
