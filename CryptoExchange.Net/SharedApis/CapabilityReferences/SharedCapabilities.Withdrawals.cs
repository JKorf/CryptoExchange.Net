using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// Withdrawal capabilities.
        /// </summary>
        public static class Withdrawals
        {
            /// <summary>
            /// Withdraw assets capability.
            /// </summary>
            public static SharedRestCapability<IWithdraw, IWithdrawRest> Withdraw { get; } = new();

            /// <summary>
            /// Get withdrawal history capability.
            /// </summary>
            public static SharedRestCapability<IGetWithdrawalHistory, IGetWithdrawalHistoryRest> GetWithdrawalHistory { get; } = new();
        }
    }
}
