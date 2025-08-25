using System;

namespace CryptoExchange.Net.Trackers.Klines;

/// <summary>
/// Klines statistics
/// </summary>
public record KlinesStats
{
    /// <summary>
    /// Number of klines
    /// </summary>
    public int KlineCount { get; set; }
    /// <summary>
    /// The kline open time of the first entry
    /// </summary>
    public DateTime? FirstOpenTime { get; set; }
    /// <summary>
    /// The kline open time of the last entry
    /// </summary>
    public DateTime? LastOpenTime { get; set; }
    /// <summary>
    /// Lowest trade price
    /// </summary>
    public decimal? LowPrice { get; set; }
    /// <summary>
    /// Highest trade price
    /// </summary>
    public decimal? HighPrice { get; set; }
    /// <summary>
    /// Trade volume
    /// </summary>
    public decimal Volume { get; set; }
    /// <summary>
    /// Average volume per kline
    /// </summary>
    public decimal? AverageVolume { get; set; }
    /// <summary>
    /// Whether the data is complete
    /// </summary>
    public bool Complete { get; set; }

    /// <summary>
    /// Compare 2 stat snapshots to each other
    /// </summary>
    public KlinesCompare CompareTo(KlinesStats otherStats)
    {
        return new KlinesCompare
        {
            LowPriceDif = new CompareValue(LowPrice, otherStats.LowPrice),
            HighPriceDif = new CompareValue(HighPrice, otherStats.HighPrice),
            VolumeDif = new CompareValue(Volume, otherStats.Volume),
            AverageVolumeDif = new CompareValue(AverageVolume, otherStats.AverageVolume),
        };
    }
}
