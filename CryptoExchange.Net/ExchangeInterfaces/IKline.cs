using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
