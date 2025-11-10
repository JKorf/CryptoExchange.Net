using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Type of ticker
    /// </summary>
    public enum SharedTickerType
    {
        /// <summary>
        /// The ticker data is calculated based on the last 24 hours
        /// </summary>
        Day24H,
        /// <summary>
        /// The ticker data is calculated based on the start of the day at 00:00 on UTC+0 
        /// </summary>
        DayUtc0,
        /// <summary>
        /// Ticker data is calculated in a different way or not consistent
        /// </summary>
        Other
    }
}
