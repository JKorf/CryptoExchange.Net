using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// Take profit and stop loss capabilities.
        /// </summary>
        public static class TpSl
        {
            /// <summary>
            /// Set futures take profit or stop loss capability.
            /// </summary>
            public static SharedRestCapability<ISetFuturesTpSl, ISetFuturesTpSlRest> SetFuturesTpSl { get; } = new();

            /// <summary>
            /// Cancel futures take profit or stop loss capability.
            /// </summary>
            public static SharedRestCapability<ICancelFuturesTpSl, ICancelFuturesTpSlRest> CancelFuturesTpSl { get; } = new();
        }
    }
}
