using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.Trades
{
    /// <summary>
    /// Trades statistics
    /// </summary>
    public record TradesStats
    {
        /// <summary>
        /// Number of trades
        /// </summary>
        public int TradeCount { get; set; }
        /// <summary>
        /// Timestamp of the last trade
        /// </summary>
        public DateTime? FirstTradeTime { get; set; }
        /// <summary>
        /// Timestamp of the first trade
        /// </summary>
        public DateTime? LastTradeTime { get; set; }
        /// <summary>
        /// Average trade price
        /// </summary>
        public decimal? AveragePrice { get; set; }
        /// <summary>
        /// Volume weighted average trade price
        /// </summary>
        public decimal? VolumeWeightedAveragePrice { get; set; }
        /// <summary>
        /// Volume of the trades
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// Volume of the trades in quote asset
        /// </summary>
        public decimal QuoteVolume { get; set; }
        /// <summary>
        /// The volume weighted Buy/Sell ratio. A 0.7 ratio means 70% of the trade volume was a buy.
        /// </summary>
        public decimal? BuySellRatio { get; set; }
        /// <summary>
        /// Whether the data is complete
        /// </summary>
        public bool Complete { get; set; }

        /// <summary>
        /// Compare 2 stat snapshots to each other
        /// </summary>
        public TradesCompare CompareTo(TradesStats otherStats)
        {
            return new TradesCompare
            {
                TradeCountDif = new CompareValue(TradeCount, otherStats.TradeCount),
                AveragePriceDif = new CompareValue(AveragePrice, otherStats.AveragePrice),
                VolumeWeightedAveragePriceDif = new CompareValue(VolumeWeightedAveragePrice, otherStats.VolumeWeightedAveragePrice),
                VolumeDif = new CompareValue(Volume, otherStats.Volume),
                QuoteVolumeDif = new CompareValue(QuoteVolume, otherStats.QuoteVolume),
                BuySellRatioDif = new CompareValue(BuySellRatio, otherStats.BuySellRatio),
            };
        }
    }
}
