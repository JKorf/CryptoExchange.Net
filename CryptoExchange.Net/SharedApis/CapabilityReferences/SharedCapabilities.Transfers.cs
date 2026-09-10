using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Transfer capabilities.
        /// </summary>
        public static class Transfers
        {
            /// <summary>
            /// Transfer assets capability.
            /// </summary>
            public static SharedRestCapability<ITransfer, ITransferRest> Transfer { get; } = new();

            /// <summary>
            /// Get transfer history capability.
            /// </summary>
            public static SharedRestCapability<IGetTransferHistory, IGetTransferHistoryRest> GetTransferHistory { get; } = new();
        }
    }
}
