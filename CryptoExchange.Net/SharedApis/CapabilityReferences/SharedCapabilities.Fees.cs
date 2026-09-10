using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Fee capabilities.
        /// </summary>
        public static class Fees
        {
            /// <summary>
            /// Get fees capability.
            /// </summary>
            public static SharedRestCapability<IGetFees, IGetFeesRest> GetFees { get; } = new();
        }

    }
}
