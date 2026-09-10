using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Deposit capabilities.
        /// </summary>
        public static class Deposits
        {
            /// <summary>
            /// Get deposit addresses capability.
            /// </summary>
            public static SharedRestCapability<IGetDepositAddresses, IGetDepositAddressesRest> GetDepositAddresses { get; } = new();
            /// <summary>
            /// Get deposit history capability.
            /// </summary>
            public static SharedRestCapability<IGetDepositHistory, IGetDepositHistoryRest> GetDepositHistory { get; } = new();
        }

    }
}
