using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Balance capabilities.
        /// </summary>
        public static class Balances
        {
            /// <summary>
            /// Get balances capability.
            /// </summary>
            public static SharedRestCapability<IGetBalances, IGetBalancesRest> GetBalances { get; } = new();
            /// <summary>
            /// Subscribe to balance updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeBalancesSocket> SubscribeBalances { get; } = new();
        }

    }
}
