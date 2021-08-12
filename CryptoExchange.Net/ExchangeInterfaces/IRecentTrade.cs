using System;

namespace CryptoExchange.Net.ExchangeInterfaces
{
    /// <summary>
    /// Recent trade
    /// </summary>
    public interface ICommonRecentTrade
    {
        /// <summary>
        /// Price of the trade
        /// </summary>
        decimal CommonPrice { get; }
        /// <summary>
        /// Quantity of the trade
        /// </summary>
        decimal CommonQuantity { get; }
        /// <summary>
        /// Trade time
        /// </summary>
        DateTime CommonTradeTime { get; }
    }
}
