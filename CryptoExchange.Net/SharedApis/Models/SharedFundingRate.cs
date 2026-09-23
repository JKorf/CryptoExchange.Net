using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Funding rate
    /// </summary>
    [DebuggerDisplay("[{Timestamp}] {FundingRate}")]
    public record SharedFundingRate
    {
        /// <summary>
        /// The funding rate
        /// </summary>
        public decimal FundingRate { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFundingRate(decimal fundingRate, DateTime timestamp)
        {
            FundingRate = fundingRate;
            Timestamp = timestamp;
        }
    }
}
