using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Funding capabilities.
        /// </summary>
        public static class Funding
        {
            /// <summary>
            /// Get current funding information capability.
            /// </summary>
            public static SharedRestCapability<IGetFundingInfo, IGetFundingInfoRest> GetFundingInfo { get; } = new();
            /// <summary>
            /// Get funding rate history capability.
            /// </summary>
            public static SharedRestCapability<IGetFundingRateHistory, IGetFundingRateHistoryRest> GetFundingRateHistory { get; } = new();
            /// <summary>
            /// Get user funding history capability.
            /// </summary>
            public static SharedRestCapability<IGetUserFundingHistory, IGetUserFundingHistoryRest> GetUserFundingHistory { get; } = new();
        }
    }
}
