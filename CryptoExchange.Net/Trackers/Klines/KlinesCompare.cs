using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.Klines
{
    /// <summary>
    /// Klines statistics comparison
    /// </summary>
    public record KlinesCompare
    {
        /// <summary>
        /// Number of trades
        /// </summary>
        public CompareValue? LowPriceDif { get; set; }
        /// <summary>
        /// Number of trades
        /// </summary>
        public CompareValue? HighPriceDif { get; set; }
        /// <summary>
        /// Number of trades
        /// </summary>
        public CompareValue? VolumeDif { get; set; }
        /// <summary>
        /// Number of trades
        /// </summary>
        public CompareValue? AverageVolumeDif { get; set; }

    }
}
