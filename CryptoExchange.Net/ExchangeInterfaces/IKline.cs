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
        decimal CommonHighPrice { get; }
        /// <summary>
        /// Low price for this kline
        /// </summary>
        decimal CommonLowPrice { get; }
        /// <summary>
        /// Open price for this kline
        /// </summary>
        decimal CommonOpenPrice { get; }
        /// <summary>
        /// Close price for this kline
        /// </summary>
        decimal CommonClosePrice { get; }
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
