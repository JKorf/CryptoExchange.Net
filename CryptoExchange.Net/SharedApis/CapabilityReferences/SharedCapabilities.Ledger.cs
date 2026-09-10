using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Ledger capabilities.
        /// </summary>
        public static class Ledger
        {
            /// <summary>
            /// Get ledger entries capability.
            /// </summary>
            public static SharedRestCapability<IGetLedger, IGetLedgerRest> GetLedger { get; } = new();
        }
    }
}
