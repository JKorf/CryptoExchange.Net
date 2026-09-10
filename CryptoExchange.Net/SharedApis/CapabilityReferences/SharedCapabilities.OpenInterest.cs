using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Open interest capabilities.
        /// </summary>
        public static class OpenInterest
        {
            /// <summary>
            /// Get open interest capability.
            /// </summary>
            public static SharedRestCapability<IGetOpenInterest, IGetOpenInterestRest> GetOpenInterest { get; } = new();
        }
    }
}
