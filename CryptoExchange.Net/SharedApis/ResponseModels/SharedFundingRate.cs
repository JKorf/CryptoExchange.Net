using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedFundingRate
    {
        public decimal FundingRate { get; set; }
        public DateTime Timestamp { get; set; }

        public SharedFundingRate(decimal fundingRate, DateTime timestamp)
        {
            FundingRate = fundingRate;
            Timestamp = timestamp;
        }
    }
}
