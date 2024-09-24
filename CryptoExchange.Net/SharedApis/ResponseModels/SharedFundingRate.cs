using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Funding rate
    /// </summary>
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
