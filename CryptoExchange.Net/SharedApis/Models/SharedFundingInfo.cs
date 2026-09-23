using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Funding info
    /// </summary>
    public record SharedFundingInfo
    {
        /// <summary>
        /// The current funding rate
        /// </summary>
        public decimal FundingRate { get; set; }
        /// <summary>
        /// Next funding timestamp
        /// </summary>
        public DateTime? NextFundingTime { get; set; }
        /// <summary>
        /// Funding interval in hours
        /// </summary>
        public int? FundingInterval { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFundingInfo(decimal fundingRate, DateTime? nextFundingTime, int? fundingInterval)
        {
            FundingRate = fundingRate;
            NextFundingTime = nextFundingTime;
            FundingInterval = fundingInterval;
        }
    }
}
